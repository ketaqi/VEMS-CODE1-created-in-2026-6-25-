using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// interpolation class
    /// </summary>
    [Obsolete("...")]
    public class Interpolation
    {

        #region Nearest-Neighbor interpolation

        #region --------- 1D Kernel [GridReal] ---------

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <returns> interpolated value f(x) </returns>
        public static double Nearest(VectorD v, GridInfo1D grid, 
            double x)
        {
            // compute distance w.r.t grid lower bound
            double d = x - grid.LowerBound;

            // check bounds
            if (d < 0.0 || d >= grid.Range)
                return 0.0;

            // nearest neighbor interpolation ...
            // find index
            long j = (long)(d / grid.Spacing);
            return v[j, false];
        }

        /// <summary>
        /// interpolates input data according to target locations
        /// by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="xe"> evaluation locations </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the evaluation locations </returns>
        /// <exception cref="NotImplementedException"></exception>
        public static VectorD Nearest(VectorD v, GridInfo1D inputGrid,
            VectorD xe,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorD ve = new(xe.Count);
            // loop
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < xe.Count; i++)
                        {
                            ve[i, false] = Nearest(v, inputGrid, xe[i, false]);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, xe.Count, i =>
                        {
                            ve[i, false] = Nearest(v, inputGrid, xe[i, false]);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return ve;
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorD Nearest(VectorD v, GridInfo1D inputGrid,
            GridInfo1D targetGrid, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorD vOut = new(targetGrid.Count);
            // loop
            switch(loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < vOut.Count; i++)
                        {
                            double x = targetGrid.GetCoordinate(i);
                            vOut[i, false] = Nearest(v, inputGrid, x);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, vOut.Count, i =>
                        {
                            double x = targetGrid.GetCoordinate(i);
                            vOut[i, false] = Nearest(v, inputGrid, x);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return vOut;
        }

        #endregion
        #region --------- 1D Kernel [GridCplx] ---------

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid for the input values </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <returns> interpolated value f(x) </returns>
        public static Complex Nearest(VectorZ v, GridInfo1D grid, 
            double x)
        {
            // compute distance w.r.t grid lower bound
            double d = x - grid.LowerBound;

            // check bounds
            if (d < 0.0 || d >= grid.Range)
                return Complex.Zero;

            // nearest neighbor interpolation ...
            // find index
            long j = (long)(d / grid.Spacing);
            return v[j, false];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="inputGrid"></param>
        /// <param name="xe"></param>
        /// <param name="loopMode"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static VectorZ Nearest(VectorZ v, GridInfo1D inputGrid,
            VectorD xe, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorZ ve = new(xe.Count);
            // loop
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < xe.Count; i++)
                        {
                            ve[i, false] = Nearest(v, inputGrid, xe[i, false]);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, xe.Count, i =>
                        {
                            ve[i, false] = Nearest(v, inputGrid, xe[i, false]);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return ve;
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorZ Nearest(VectorZ v, GridInfo1D inputGrid,
            GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // extracts real and imag-parts
            VectorD vRe = new(inputGrid.Count);
            VectorD vIm = new(inputGrid.Count);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            VectorD yRe = Nearest(vRe, inputGrid, targetGrid, loopMode);
            VectorD yIm = Nearest(vIm, inputGrid, targetGrid, loopMode);
            // return 
            return VMath.Construct(yRe, yIm);
        }

        #endregion
        #region --------- 2D Kernel [GridReal] ---------

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static double Nearest(MatrixD v, GridInfo2D grid, 
            double x, double y)
        {
            // compute distance
            double dy = y - grid.LowerBoundY;
            double dx = x - grid.LowerBoundX;

            // check bounds
            if (dy < 0.0 || dy >= grid.RangeY
                || dx < 0.0 || dx >= grid.RangeX)
                return 0.0;

            // nearest-neighbor interpolation computation ...
            // indices
            long jCol = (long)(dx / grid.SpacingX);
            long jRow = (long)(dy / grid.SpacingY);

            return v[jRow, jCol, false];
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on target grid </returns>
        public static MatrixD Nearest(MatrixD v, GridInfo2D inputGrid,
            GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            MatrixD b = new(targetGrid.Rows, targetGrid.Cols, initVal: 0.0);
            // loop
            switch(loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long iRow = 0; iRow < b.Rows; iRow++)
                        {
                            double y = targetGrid.GetCoordinateY(iRow);
                            for (long iCol = 0; iCol < b.Cols; iCol++)
                            {
                                double x = targetGrid.GetCoordinateX(iCol);
                                b[iRow, iCol, false] = Nearest(v, inputGrid, x, y);
                            }
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, b.Rows, iRow =>
                        {
                            double y = targetGrid.GetCoordinateY(iRow);
                            for (long iCol = 0; iCol < b.Cols; iCol++)
                            {
                                double x = targetGrid.GetCoordinateX(iCol);
                                b[iRow, iCol, false] = Nearest(v, inputGrid, x, y);
                            }
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return b;
        }

        #endregion
        #region --------- 2D Kernel [GridCplx] ---------

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by nearest-neighbor interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static Complex Nearest(MatrixZ v, GridInfo2D grid, 
            double x, double y)
        {
            // extracts real and imag-parts
            MatrixD vRe = new(grid.Rows, grid.Cols);
            MatrixD vIm = new(grid.Rows, grid.Cols);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            double yRe = Nearest(vRe, grid, x, y);
            double yIm = Nearest(vIm, grid, x, y);
            // return
            return new(yRe, yIm);
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on target grid </returns>
        public static MatrixZ Nearest(MatrixZ v, GridInfo2D inputGrid,
            GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // extracts real and imag-parts
            MatrixD vRe = new(inputGrid.Rows, inputGrid.Cols);
            MatrixD vIm = new(inputGrid.Rows, inputGrid.Cols);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            MatrixD yRe = Nearest(vRe, inputGrid, targetGrid, loopMode);
            MatrixD yIm = Nearest(vIm, inputGrid, targetGrid, loopMode);
            // return
            return VMath.Construct(yRe, yIm);
        }

        #endregion
        //#region --------- wrappers ---------

        ///// <summary>
        ///// finds the value f(x) for given variable x
        ///// by nearest-neighbor interpolation
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <returns> interpolated value f(x) </returns>
        //public static double Nearest(Grid1DRealData v, double x)
        //    => Nearest(v.Values, v.Grid, x);

        ///// <summary>
        ///// finds the value f(x) for given variable x
        ///// by nearest-neighbor interpolation
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <returns> interpolated value f(x) </returns>
        //public static Complex Nearest(Grid1DCplxData v, double x)
        //    => Nearest(v.Values, v.Grid, x);

        ///// <summary>
        ///// interpolates input data according to a target uniform grid
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on the target grid </returns>
        //public static VectorD Nearest(Grid1DRealData v,
        //    GridInfo1D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Nearest(v.Values, v.Grid, targetGrid, loopMode);

        ///// <summary>
        ///// interpolates input data according to a target uniform grid
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on the target grid </returns>
        //public static VectorZ Nearest(Grid1DCplxData v,
        //    GridInfo1D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Nearest(v.Values, v.Grid, targetGrid, loopMode);

        ///// <summary>
        ///// finds the value f(x, y) for given 
        ///// x and y, by nearest-neighbor interpolation
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <param name="y"> arbitrary variable y </param>
        ///// <returns> interpolated value f(x, y) </returns>
        //public static double Nearest(Grid2DRealData v, double x, double y)
        //    => Nearest(v.Values, v.Grid, x, y);

        ///// <summary>
        ///// finds the value f(x, y) for given 
        ///// x and y, by nearest-neighbor interpolation
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <param name="y"> arbitrary variable y </param>
        ///// <returns> interpolated value f(x, y) </returns>
        //public static Complex Nearest(Grid2DCplxData v, double x, double y)
        //    => Nearest(v.Values, v.Grid, x, y);

        ///// <summary>
        ///// interpolates input data according to a target uniform grid
        ///// </summary>
        ///// <param name="v"> uniformly sampled input values </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on target grid </returns>
        //public static MatrixD Nearest(Grid2DRealData v,
        //    GridInfo2D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Nearest(v.Values, v.Grid, targetGrid, loopMode);

        ///// <summary>
        ///// interpolates input data according to a target uniform grid
        ///// </summary>
        ///// <param name="v"> uniformly sampled input values </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on target grid </returns>
        //public static MatrixZ Nearest(Grid2DCplxData v,
        //    GridInfo2D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Nearest(v.Values, v.Grid, targetGrid, loopMode);

        //#endregion

        #endregion
        #region (Bi-)Linear interpolation

        #region --------- 1D Kernel [GridReal] ---------

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid </param>
        /// <param name="x"> target location x </param>
        /// <returns> interpolated value y = f(x) </returns>
        public static double Linear(VectorD v, GridInfo1D grid, double x)
        {
            // compute distance w.r.t 1st grid point
            double d = x - grid.Start;

            // check bounds
            if (d < 0.0 || d >= grid.Range - grid.Spacing)
                return 0.0;

            // linear interpolation computation ...
            // lower index
            long j = (long)(d / grid.Spacing);
            // local distance w.r.t. lower index
            double td = x - grid.GetCoordinate(j);
            if (td == 0.0) // coincide
                return v[j, false];
            else // apply linear interpolation
            {
                double t = td * v[j + 1, false]
                    + (grid.Spacing - td) * v[j, false];
                return t / grid.Spacing;
            }
        }

        /// <summary>
        /// interpolates input data according to target locations
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="xe"> evaluation locations </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the evaluation locations </returns>
        /// <exception cref="NotImplementedException"></exception>
        public static VectorD Linear(VectorD v, GridInfo1D inputGrid,
            VectorD xe,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorD ve = new(xe.Count);
            // loop
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long i = 0; i < xe.Count; i++)
                        {
                            ve[i, false] = Linear(v, inputGrid, xe[i, false]);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, xe.Count, i =>
                        {
                            ve[i, false] = Linear(v, inputGrid, xe[i, false]);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return ve;
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorD Linear(VectorD v, GridInfo1D inputGrid,
            GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Linear(v: v, inputGrid: inputGrid, 
                xe: targetGrid.GetCoordinates(), 
                loopMode: loopMode);

        #endregion
        #region --------- 1D Kernel [ScatReal] ---------

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="xs"> scattered input sample locations </param>
        /// <param name="vs"> sampled input data values </param>
        /// <param name="x"> target location x </param>
        /// <returns> interpolated value y = f(x) </returns>
        public static double Linear(VectorD xs, VectorD vs, double x)
        {
            bool spanFound = Search.Binary(xs, x, out long ix);
            if (!spanFound) { return 0.0; }
            else if(ix == xs.Count - 1) { return vs[xs.Count - 1, false]; }
            else
            {
                double td = x - xs[ix, false];
                if (td == 0.0) { return vs[ix, false]; }
                else // applies linear interpolation
                {
                    double d = xs[ix + 1, false] - xs[ix, false];
                    double t = td * vs[ix + 1, false] + (d - td) * vs[ix, false];
                    return t / d;
                }
            }
        }

        /// <summary>
        /// interpolates input data according to target locations
        /// </summary>
        /// <param name="xs"> scattered input sample locations </param>
        /// <param name="vs"> sampled input data values </param>
        /// <param name="xe"> evaluation locations </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the evaluation locations </returns>
        /// <exception cref="NotImplementedException"></exception>
        public static VectorD Linear(VectorD xs, VectorD vs,
            VectorD xe,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            VectorD ve = new(xe.Count);
            // loop
            switch (loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for(long i = 0; i < xe.Count; i++)
                        {
                            ve[i, false] = Linear(xs, vs, xe[i, false]);
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, xe.Count, i =>
                        {
                            ve[i, false] = Linear(xs, vs, xe[i, false]);
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return ve;
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="xs"> scattered input sample locations </param>
        /// <param name="vs"> sampled input data values </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorD Linear(VectorD xs, VectorD vs,
            GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
            => Linear(xs: xs, vs: vs,
                xe: targetGrid.GetCoordinates(),
                loopMode: loopMode);

        #endregion
        #region --------- 1D Kernel [GridCplx] ---------

        /// <summary>
        /// finds the value f(x) for given variable x
        /// by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <returns> interpolated value f(x) </returns>
        public static Complex Linear(VectorZ v, GridInfo1D grid, double x)
        {
            // gets the input real and imag-parts
            VectorD vRe = new(v.Count);
            VectorD vIm = new(v.Count);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            double yRe = Linear(vRe, grid, x);
            double yIm = Linear(vIm, grid, x);
            // return
            return new(yRe, yIm);
        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on the target grid </returns>
        public static VectorZ Linear(VectorZ v, GridInfo1D inputGrid,
            GridInfo1D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // gets the input real and imag-parts
            VectorD vRe = new(v.Count);
            VectorD vIm = new(v.Count);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            VectorD yRe = Linear(vRe, inputGrid, targetGrid, loopMode);
            VectorD yIm = Linear(vIm, inputGrid, targetGrid, loopMode);
            // return
            return VMath.Construct(yRe, yIm);
        }

        #endregion
        #region --------- 2D Kernel [GridReal] ---------

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static double BiLinear(MatrixD v, GridInfo2D grid, 
            double x, double y)
        {
            // compute distance
            double dy = y - grid.StartY;
            double dx = x - grid.StartX;

            // check bounds
            if (dy < 0.0 || dy >= grid.RangeY - grid.SpacingY
                || dx < 0.0 || dx >= grid.RangeX - grid.SpacingX)
                return 0.0;

            // linear interpolation computation ...
            // left-lower index
            long jCol = (long)(dx / grid.SpacingX);
            long jRow = (long)(dy / grid.SpacingY);
            // local distance w.r.t. left-lower corner
            double tdx = x - grid.GetCoordinateX(jCol);
            double tdy = y - grid.GetCoordinateY(jRow);

            if (tdx == 0.0 && tdy == 0.0) // coincide
                return v[jRow, jCol, false];
            else if (tdx == 0.0) // => linear 1D
            {
                // only need to interpolate along y
                double t = tdy * v[jRow + 1, jCol, false]
                    + (grid.SpacingY - tdy) * v[jRow, jCol, false];
                return t / grid.SpacingY;
            }
            else if (tdy == 0.0) // => linear 1D
            {
                // only need to interpolate along x
                double t = tdx * v[jRow, jCol + 1, false]
                    + (grid.SpacingX - tdx) * v[jRow, jCol, false];
                return t / grid.SpacingX;
            }
            else
            {
                // need two interpolations along x 
                double t1 = tdx * v[jRow, jCol + 1, false]
                    + (grid.SpacingX - tdx) * v[jRow, jCol, false];
                t1 /= grid.SpacingX;
                double t2 = tdx * v[jRow + 1, jCol + 1, false]
                    + (grid.SpacingX - tdx) * v[jRow + 1, jCol, false];
                t2 /= grid.SpacingX;
                // and one interpolation along y
                double t = tdy * t2 + (grid.SpacingY - tdy) * t1;
                return t / grid.SpacingY;
            }

        }

        /// <summary>
        /// interpolates input data according to target uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on target grid </returns>
        public static MatrixD BiLinear(MatrixD v, GridInfo2D inputGrid, 
            GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialize
            MatrixD b = new(targetGrid.Rows, targetGrid.Cols);
            // loop
            switch(loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for (long iRow = 0; iRow < b.Rows; iRow++)
                        {
                            double y = targetGrid.GetCoordinateY(iRow);
                            for (long iCol = 0; iCol < b.Cols; iCol++)
                            {
                                double x = targetGrid.GetCoordinateX(iCol);
                                b[iRow, iCol, false] = BiLinear(v, inputGrid, x, y);
                            }
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, b.Rows, iRow =>
                        {
                            double y = targetGrid.GetCoordinateY(iRow);
                            for (long iCol = 0; iCol < b.Cols; iCol++)
                            {
                                double x = targetGrid.GetCoordinateX(iCol);
                                b[iRow, iCol, false] = BiLinear(v, inputGrid, x, y);
                            }
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
            // return
            return b;
        }

        #endregion
        #region --------- 2D Kernel [GridCplx] ---------

        /// <summary>
        /// finds the value f(x, y) for given 
        /// x and y, by linear interpolation
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="grid"> sampling grid </param>
        /// <param name="x"> arbitrary variable x </param>
        /// <param name="y"> arbitrary variable y </param>
        /// <returns> interpolated value f(x, y) </returns>
        public static Complex BiLinear(MatrixZ v, GridInfo2D grid, 
            double x, double y)
        {
            // extracts real and imag-parts
            MatrixD vRe = new(grid.Rows, grid.Cols);
            MatrixD vIm = new(grid.Rows, grid.Cols);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            double yRe = BiLinear(vRe, grid, x, y);
            double yIm = BiLinear(vIm, grid, x, y);
            // return
            return new (yRe, yIm);
        }

        /// <summary>
        /// interpolation data according to another uniform grid
        /// </summary>
        /// <param name="v"> uniformly sampled input values </param>
        /// <param name="inputGrid"> input sampling grid </param>
        /// <param name="targetGrid"> target sampling grid </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> interpolated values on target grid </returns>
        public static MatrixZ BiLinear(MatrixZ v, GridInfo2D inputGrid,
            GridInfo2D targetGrid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // extracts real and imag-parts
            MatrixD vRe = new(inputGrid.Rows, inputGrid.Cols);
            MatrixD vIm = new(inputGrid.Rows, inputGrid.Cols);
            VMath.RealImagParts(v, ref vRe, ref vIm);
            // interpolates respectively
            MatrixD yRe = BiLinear(vRe, inputGrid, targetGrid, loopMode);
            MatrixD yIm = BiLinear(vIm, inputGrid, targetGrid, loopMode);
            // return
            return VMath.Construct(yRe, yIm);
        }

        #endregion
        //#region --------- wrappers ---------

        ///// <summary>
        ///// finds the value f(x) for given variable x
        ///// by linear interpolation
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <returns> interpolated value f(x) </returns>
        //public static double Linear(Grid1DRealData v, double x)
        //    => Linear(v.Values, v.Grid, x);

        ///// <summary>
        ///// finds the value f(x) for given variable x
        ///// by linear interpolation
        ///// </summary>
        ///// <param name="v"> uniformly sampled input data </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <returns> interpolated value f(x) </returns>
        //public static Complex Linear(Grid1DCplxData v, double x)
        //    => Linear(v.Values, v.Grid, x);

        ///// <summary>
        ///// finds the value f(x, y) for given
        ///// x and y, by linear interpolation
        ///// </summary>
        ///// <param name="a"> uniformly sampled input values </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <param name="y"> arbitrary variable y </param>
        ///// <returns> interpolated value f(x, y) </returns>
        //public static double BiLinear(Grid2DRealData a, double x, double y)
        //    => BiLinear(a.Values, a.Grid, x, y);

        ///// <summary>
        ///// finds the value f(x, y) for given
        ///// x and y, by linear interpolation
        ///// </summary>
        ///// <param name="a"> uniformly sampled input values </param>
        ///// <param name="x"> arbitrary variable x </param>
        ///// <param name="y"> arbitrary variable y </param>
        ///// <returns> interpolated value f(x, y) </returns>
        //public static Complex BiLinear(Grid2DCplxData a, double x, double y)
        //    => BiLinear(a.Values, a.Grid, x, y);


        ///// <summary>
        ///// interpolates inpu data according to target uniform grid
        ///// </summary>
        ///// <param name="a"> uniformly sampled input data </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on target grid </returns>
        //public static VectorD Linear(Grid1DRealData a, GridInfo1D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Linear(a.Values, a.Grid, targetGrid, loopMode);

        ///// <summary>
        ///// interpolates inpu data according to target uniform grid
        ///// </summary>
        ///// <param name="a"> uniformly sampled input data </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on target grid </returns>
        //public static VectorZ Linear(Grid1DCplxData a, GridInfo1D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Linear(a.Values, a.Grid, targetGrid, loopMode);

        ///// <summary>
        ///// interpolates input data according to target uniform grid
        ///// </summary>
        ///// <param name="a"> uniformly sampled input data </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on target grid </returns>
        //public static MatrixD BiLinear(Grid2DRealData a, GridInfo2D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => BiLinear(a.Values, a.Grid, targetGrid, loopMode);

        ///// <summary>
        ///// interpolates input data according to target uniform grid
        ///// </summary>
        ///// <param name="a"> uniformly sampled input data </param>
        ///// <param name="targetGrid"> target sampling grid </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> interpolated values on target grid </returns>
        //public static MatrixZ BiLinear(Grid2DCplxData a, GridInfo2D targetGrid,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => BiLinear(a.Values, a.Grid, targetGrid, loopMode);

        //#endregion

        #endregion
        // (Bi-)Cubic interpolation

    }

    /// <summary>
    /// scat-output interpolation
    /// </summary>
    public class ScatInterpolation
    {

    }


}
