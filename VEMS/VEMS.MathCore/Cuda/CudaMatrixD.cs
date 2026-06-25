using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MathCore
{

    /// <summary>
    /// real-valued matrix on Cuda device
    /// </summary>
    public unsafe class CudaMatrixD : CudaArray<double>
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
        /// constructs an empty CudaMatrixD
        /// with given rows and columns
        /// </summary>
        /// <param name="rows"> number of rows </param>
        /// <param name="cols"> number of columns </param>
        public CudaMatrixD(long rows, long cols) : base(rows * cols) 
        {
            Rows = rows;
            Cols = cols;
        }

        /// <summary>
        /// constructs a CudaMatrix from a given
        /// matrix x in the host memory space
        /// </summary>
        /// <param name="x"> host matrix x as the source </param>
        public CudaMatrixD(MatrixD x) : this(x.Rows, x.Cols)
            => SetValues(x);

        #endregion
        #region methods

        /// <summary>
        /// copies a matrix x in host memory space 
        /// to the matrix in device memory space
        /// </summary>
        /// <param name="x"> host matrix x as the source </param>
        public void SetValues(MatrixD x)
        {
            // first, transposes x, pretents to be column-major
            MatrixD xt = LinAlg.Transpose(x);
            //Printer.Write($"xt = ", xt);
            //xt.Rows = x.Cols;
            //xt.Cols = x.Rows;
            // then, sets the matrix
            //CudaHelper.SetMatrix(x.Rows, x.Cols, sizeof(double), xt.VPtr, xt.Rows, VPtr, Rows);
            CudaHelper.SetVector(x.Count, sizeof(double), xt.VPtr, 1, VPtr, 1);
        }

        /// <summary>
        /// copies the matrix in device memory space
        /// to a matrix y in host memory space 
        /// </summary>
        /// <param name="y"> host matrix y as the target </param>
        private void GetValues(ref MatrixD y)
        {
            // first, gets the matrix
            CudaHelper.GetMatrix(Rows, Cols, sizeof(double), VPtr, Rows, y.VPtr, y.Rows);
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
        public MatrixD GetValues()
        {
            // first, prepares a transposed container 
            MatrixD yt = new(Cols, Rows);
            // then, gets the matrix to the transposed container
            //CudaHelper.GetMatrix(Rows, Cols, sizeof(double), VPtr, Rows, yt.VPtr, yt.Rows);
            CudaHelper.GetVector(Count, sizeof(double), VPtr, 1, yt.VPtr, 1);
            //Printer.Write($"yt = ", yt);
            // finally, transpose the matrix
            LinAlg.Transpose(ref yt);
            //MatrixD y = LinAlg.Transpose(yt);
            return yt;
        }

        /// <summary>
        /// computes the sum of the absolute values of all elements
        /// </summary>
        /// <returns> sum of absolute values </returns>
        public double ASum()
            => CudaVMath.AbsSum(this);

        #endregion
    }
}
