using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// pointwise finite-difference class
    /// </summary>
    public class PointFiDi
    {
        #region ------- 1D cases -------

        #region ===== Dt =====

        /// <summary>
        /// computes 1st-order difference
        /// [central difference except for border indices]
        /// at a specific index
        /// </summary>
        /// <param name="vs"> input values </param>
        /// <param name="i"> target index for the derivative evaluation </param>
        /// <param name="grid"> uniform sampling grid of the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 1st-order derivative </returns>
        public static double Dt(VectorD vs, long i,
            GridInfo1D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if (checkBound)
            { if (i < 0 || i >= vs.Count) { d = 0.0; } }
            // within bound
            if (i == 0) // forced to forward difference
                d = vs[i + 1, false] - vs[i, false];
            else if (i == vs.Count - 1) // forced to backward difference
                d = vs[i, false] - vs[i - 1, false];
            else // using central difference
                d = (vs[i + 1, false] - vs[i - 1, false]) / 2.0;

            if(grid != null) { d /= grid.Spacing; }
            return d;
        }

        /// <summary>
        /// computes 1st-order difference
        /// [central difference except for border indices]
        /// at a specific index
        /// </summary>
        /// <param name="vs"> input values </param>
        /// <param name="i"> target index for the derivative evaluation </param>
        /// <param name="grid"> uniform sampling grid of the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 1st-order derivative </returns>
        public static Complex Dt(VectorZ vs, long i,
            GridInfo1D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            { if (i < 0 || i >= vs.Count) { d = Complex.Zero; } }
            // within bound
            if (i == 0) // forced to forward difference
                d = vs[i + 1, false] - vs[i, false];
            else if (i == vs.Count - 1) // forced to backward difference
                d = vs[i, false] - vs[i - 1, false];
            else // using central difference
                d = (vs[i + 1, false] - vs[i - 1, false]) / 2.0;

            if (grid != null) { d /= grid.Spacing; }
            return d;
        }

        #endregion
        #region ===== Dtt =====

        /// <summary>
        /// computes 2nd-order difference
        /// [central difference except for border indices]
        /// at a specific index
        /// </summary>
        /// <param name="vs"> input values </param>
        /// <param name="i"> target index for the derivative evaluation </param>
        /// <param name="grid"> uniform sampling grid of the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 2nd-order derivative </returns>
        public static double Dtt(VectorD vs, long i,
            GridInfo1D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if(checkBound)
            { if (i < 0 || i >= vs.Count) { d = 0.0; } }
            // within bound
            if (i == 0) // forced to forward difference
                d = vs[i + 2, false] + vs[i, false] - 2.0 * vs[i + 1, false];
            else if (i == vs.Count - 1) // forced to backward difference
                d = vs[i, false] + vs[i - 2, false] - 2.0 * vs[i - 1, false];
            else // using central difference
                d = (vs[i + 1, false] + vs[i - 1, false] - 2.0 * vs[i, false]);
        
            if(grid != null) { d /= (grid.Spacing * grid.Spacing); }
            return d;
        }

        /// <summary>
        /// computes 2nd-order difference
        /// [central difference except for border indices]
        /// at a specific index
        /// </summary>
        /// <param name="vs"> input values </param>
        /// <param name="i"> target index for the derivative evaluation </param>
        /// <param name="grid"> uniform sampling grid of the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 2nd-order derivative </returns>
        public static Complex Dtt(VectorZ vs, long i,
            GridInfo1D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            { if (i < 0 || i >= vs.Count) { d = Complex.Zero; } }
            // within bound
            if (i == 0) // forced to forward difference
                d = vs[i + 2, false] + vs[i, false] - 2.0 * vs[i + 1, false];
            else if (i == vs.Count - 1) // forced to backward difference
                d = vs[i, false] + vs[i - 2, false] - 2.0 * vs[i - 1, false];
            else // using central difference
                d = (vs[i + 1, false] + vs[i - 1, false] - 2.0 * vs[i, false]);
       
            if(grid != null) { d /= (grid.Spacing * grid.Spacing); }
            return d;
        }

        #endregion

        #endregion
        #region ------- 2D cases -------

        #region ===== Dx =====        

        /// <summary>
        /// computes 1st-order central difference along x
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 1st-order derivative along x at the specific location </returns>
        public static double Dx(MatrixD vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = 0.0; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = 0.0; }
            }
            // within bound
            if (iCol == 0) // force to forward difference
                d = vs[iRow, iCol + 1, false] - vs[iRow, iCol, false]; // / dx;
            else if (iCol == vs.Cols - 1) // force to backward difference
                d = vs[iRow, iCol, false] - vs[iRow, iCol - 1, false]; // / dx;
            else // using central difference
                d = (vs[iRow, iCol + 1, false] - vs[iRow, iCol - 1, false]) / 2.0; // / (2.0 * dx);
        
            if(grid != null) { d /= grid.SpacingX; }
            return d;
        }

        /// <summary>
        /// computes 1st-order central difference along x
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 1st-order derivative along x at the specific location </returns>
        public static Complex Dx(MatrixZ vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = 0.0; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = 0.0; }
            }
            // within bound
            if (iCol == 0) // force to forward difference
                d = vs[iRow, iCol + 1, false] - vs[iRow, iCol, false]; // / dx;
            else if (iCol == vs.Cols - 1) // force to backward difference
                d = vs[iRow, iCol, false] - vs[iRow, iCol - 1, false]; // / dx;
            else // using central difference
                d = (vs[iRow, iCol + 1, false] - vs[iRow, iCol - 1, false]) / 2.0; // / (2.0 * dx);

            if (grid != null) { d /= grid.SpacingX; }
            return d;
        }

        #endregion
        #region ===== Dy =====

        /// <summary>
        /// computes 1st-order central difference along y
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 1st-order derivative along y at the specific location </returns>
        public static double Dy(MatrixD vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = 0.0; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = 0.0; }
            }
            // within bound
            if (iRow == 0) // force to forward difference
                d = vs[iRow + 1, iCol, false] - vs[iRow, iCol, false]; //) / dy;
            else if (iRow == vs.Rows - 1) // force to backward difference
                d = vs[iRow, iCol, false] - vs[iRow - 1, iCol, false]; //) / dy;
            else // using central difference
                d = (vs[iRow + 1, iCol, false] - vs[iRow - 1, iCol, false]) / 2.0; // / (2.0 * dy);
        
            if(grid != null) { d /= grid.SpacingY; }
            return d;
        }

        /// <summary>
        /// computes 1st-order central difference along y
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 1st-order derivative along y at the specific location </returns>
        public static Complex Dy(MatrixZ vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = Complex.Zero; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = Complex.Zero; }
            }
            // within bound
            if (iRow == 0) // force to forward difference
                d = vs[iRow + 1, iCol, false] - vs[iRow, iCol, false]; //) / dy;
            else if (iRow == vs.Rows - 1) // force to backward difference
                d = vs[iRow, iCol, false] - vs[iRow - 1, iCol, false]; // / dy;
            else // using central difference
                d = (vs[iRow + 1, iCol, false] - vs[iRow - 1, iCol, false]) / 2.0; // ; / (2.0 * dy);
        
            if(grid != null) { d /= grid.SpacingY; }
            return d;
        }

        #endregion
        #region ===== Dxx =====

        /// <summary>
        /// computes 2nd-order central difference along x
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 2nd-order derivative along x at the specific location </returns>
        public static double Dxx(MatrixD vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = 0.0; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = 0.0; }
            }
            // within bound
            if (iCol == 0) // force to forward difference
                d = vs[iRow, iCol + 2, false] + vs[iRow, iCol, false] - 2.0 * vs[iRow, iCol + 1, false]; // / (dx * dx);
            else if (iCol == vs.Cols - 1) // force to backward difference
                d = vs[iRow, iCol, false] + vs[iRow, iCol - 2, false] - 2.0 * vs[iRow, iCol - 1, false]; // / (dx * dx);
            else // using central difference
                d = vs[iRow, iCol + 1, false] + vs[iRow, iCol - 1, false] - 2.0 * vs[iRow, iCol, false]; //) / (dx * dx);
        
            if(grid != null) { d /= (grid.SpacingX * grid.SpacingX); }
            return d;
        }

        /// <summary>
        /// computes 2nd-order central difference along x
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 2nd-order derivative along x at the specific location </returns>
        public static Complex Dxx(MatrixZ vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = Complex.Zero; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = Complex.Zero; }
            }
            // within bound
            if (iCol == 0) // force to forward difference
                d = vs[iRow, iCol + 2, false] + vs[iRow, iCol, false] - 2.0 * vs[iRow, iCol + 1, false]; // / (dx * dx);
            else if (iCol == vs.Cols - 1) // force to backward difference
                d = vs[iRow, iCol, false] + vs[iRow, iCol - 2, false] - 2.0 * vs[iRow, iCol - 1, false]; // / (dx * dx);
            else // using central difference
                d = vs[iRow, iCol + 1, false] + vs[iRow, iCol - 1, false] - 2.0 * vs[iRow, iCol, false]; // / (dx * dx);
        
            if(grid != null) { d /= (grid.SpacingX * grid.SpacingX); }
            return d;
        }

        #endregion
        #region ===== Dyy =====

        /// <summary>
        /// computes 2nd-order central difference along y
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 2nd-order derivative along y at the specific location </returns>
        public static double Dyy(MatrixD vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = 0.0; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = 0.0; }
            }
            // within bound
            if (iRow == 0) // force to forward difference
                d = vs[iRow + 2, iCol, false] + vs[iRow, iCol, false] - 2.0 * vs[iRow + 1, iCol, false]; // / (dy * dy);
            else if (iRow == vs.Rows - 1) // force to backward difference
                d = vs[iRow, iCol, false] + vs[iRow - 2, iCol, false] - 2.0 * vs[iRow - 1, iCol, false]; // / (dy * dy);
            else // using central difference
                d = vs[iRow + 1, iCol, false] + vs[iRow - 1, iCol, false] - 2.0 * vs[iRow, iCol, false]; //) / (dy * dy);
        
            if(grid != null) { d /= (grid.SpacingY * grid.SpacingY); }
            return d;
        }

        /// <summary>
        /// computes 2nd-order central difference along y
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> 2nd-order derivative along y at the specific location </returns>
        public static Complex Dyy(MatrixZ vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = Complex.Zero; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = Complex.Zero; }
            }
            // within bound
            if (iRow == 0) // force to forward difference
                d = vs[iRow + 2, iCol, false] + vs[iRow, iCol, false] - 2.0 * vs[iRow + 1, iCol, false]; // / (dy * dy);
            else if (iRow == vs.Rows - 1) // force to backward difference
                d = vs[iRow, iCol, false] + vs[iRow - 2, iCol, false] - 2.0 * vs[iRow - 1, iCol, false]; // / (dy * dy);
            else // using central difference
                d = vs[iRow + 1, iCol, false] + vs[iRow - 1, iCol, false] - 2.0 * vs[iRow, iCol, false]; // / (dy * dy);
         
            if(grid != null) { d /= (grid.SpacingY * grid.SpacingY); }
            return d;
        }

        #endregion
        #region ===== Dxy =====

        /// <summary>
        /// computes the cross difference along x first and then along y
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> cross-derivative along x first and then along y </returns>
        public static double Dxy(MatrixD vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            double d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = 0.0; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = 0.0; }
            }
            // within bound
            // cases
            if (iRow == 0) // forward difference along y
            {
                //long iRowp = iRow + 1;
                double dxp = Dx(vs, iRow + 1, iCol, grid, checkBound);
                double dx = Dx(vs, iRow, iCol, grid, checkBound);
                d = dxp - dx; // /dy;
            }
            else if (iRow == vs.Rows - 1) // backward difference along y
            {
                //long iym = iRow - 1;
                double dx = Dx(vs, iRow, iCol, grid, checkBound);
                double dxm = Dx(vs, iRow - 1, iCol, grid, checkBound);
                d = dx - dxm; // / dy;
            }
            else // central difference along y
            {
                //long iyp = iy + 1;
                //long iym = iy - 1;
                double dxp = Dx(vs, iRow + 1, iCol, grid, checkBound);
                double dxm = Dx(vs, iRow - 1, iCol, grid, checkBound);
                d = (dxp - dxm) / 2.0; // (2.0 * dy);
            }

            if(grid != null) { d /= grid.SpacingY; }
            return d;
        }

        /// <summary>
        /// computes the cross difference along x first and then along y
        /// at an index-specified location
        /// </summary>
        /// <param name="vs"> uniformly sampled input values </param>
        /// <param name="iRow"> target row index along y </param>
        /// <param name="iCol"> target column index along x </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="checkBound"> whether to check if the index is outside bound </param>
        /// <returns> cross-derivative along x first and then along y </returns>
        public static Complex Dxy(MatrixZ vs, long iRow, long iCol,
            GridInfo2D? grid = null,
            bool checkBound = false)
        {
            Complex d;

            // outside bound?
            if (checkBound)
            {
                if (iCol < 0 || iCol > vs.Cols - 1) { d = Complex.Zero; }
                if (iRow < 0 || iRow > vs.Rows - 1) { d = Complex.Zero; }
            }
            // within bound
            // cases
            if (iRow == 0) // forward difference along y
            {
                //long iyp = iy + 1;
                Complex dxp = Dx(vs, iRow + 1, iCol, grid, checkBound);
                Complex dx = Dx(vs, iRow, iCol, grid, checkBound);
                d = dxp - dx; // / dy;
            }
            else if (iRow == vs.Rows - 1) // backward difference along y
            {
                //long iym = iy - 1;
                Complex dx = Dx(vs, iRow, iCol, grid, checkBound);
                Complex dxm = Dx(vs, iRow - 1, iCol, grid, checkBound);
                d = dx - dxm; // / dy;
            }
            else // central difference along y
            {
                //long iyp = iy + 1;
                //long iym = iy - 1;
                Complex dxp = Dx(vs, iRow +1, iCol, grid, checkBound);
                Complex dxm = Dx(vs, iRow - 1, iCol, grid, checkBound);
                d = (dxp - dxm) / 2.0; // (2.0 * dy);
            }

            if(grid != null) { d /= grid.SpacingY; }
            return d;
        }

        #endregion
        #endregion
    }

}
