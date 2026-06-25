using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// helper methods for eigen solutions
    /// </summary>
    internal class EigenHelper
    {
        #region === spatial frequecy generation ===

        /// <summary>
        /// generates 1D uniformly distributed transverse
        /// spatial frequencies in a vector
        /// </summary>
        /// <param name="n"> number of total spatial frequencies </param>
        /// <param name="dk"> uniform distance in k-domain </param>
        /// <param name="kc"> center shift </param>
        /// <returns> spatial frequency vector </returns>
        internal static VectorD GenerateKs(long n, double dk,
            double kc = 0.0)
        {
            // consistency check
            if (n % 2 == 0) { Printer.Warning("n is usually supposed to be odd ..."); }
            // generate k vector
            VectorD vk = new(count: n,
                initVal: -(n - 1) / 2 * dk + kc,
                increment: dk);
            return vk;
        }

        #endregion
        #region === eigenvalue checker ===

        /// <summary>
        /// checks the imaginary part of gamma and
        /// makes sure that there is no exponential growth
        /// </summary>
        internal static void CheckGamma(ref VectorZ gamma, double tolerance = 1e-5)
        {
            for (int i = 0; i < gamma.Count; i++)
            {
                double Re = gamma[i].Real;
                double Im = gamma[i].Imaginary;
                if (Re > 0)// reverse the sign, when realpart of gamma is positive
                {
                    gamma[i] = -gamma[i];
                }
                else if (Math.Abs(Re) <= tolerance)
                {
                    if (Im < 0) // in fact, this does not matter
                        gamma[i] = -gamma[i];
                }
            }
        }

        #endregion
        #region === toeplitz matrix ===

        /// <summary>
        /// helper function to generate a toeplitz matrix
        /// from e.g. epsilon or mu vector
        /// </summary>
        /// <param name="input"> input vector </param>
        /// <returns> toeplitz matrix </returns>
        internal static MatrixZ MakeToeplitzMatrix(VectorZ input)
        {
            long mid = (input.Count - 1) / 2;
            VectorZ firstCol = new(mid + 1); //, 0.0);
            VectorZ firstRow = new(mid + 1); //, 0.0);
            for (long i = 0; i < mid + 1; i++)
            {
                firstCol[i] = input[mid + i];
                firstRow[i] = input[mid - i];
            }
            MatrixZ tpz = LinAlg.GenerateToeplitzMatrixZ(firstCol, firstRow);
            return tpz;
        }

        /// <summary>
        /// Generate a Toeplitz vector from the input vector,
        /// according to the sample count.
        /// </summary>
        /// <param name="input">input epsilon or mu in spatial-domain </param>
        /// <param name="nKxs">Number of spatial frequencies for the Toeplitz matrix.</param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns>get the toeplitz value.</returns>
        internal static VectorZ ToeplitzValue(VectorZ input, long nKxs, ToeplitzMatrixType toeplitztype)
        {
            VectorZ y = new(other: input, deepCopy: true);
            long n = 2 * nKxs - 1;

            switch (toeplitztype)
            {
                case ToeplitzMatrixType.Periodic:
                    {
                        y = y.Replicate(targetCount: 3 * y.Count);
                        y = y.Truncate(targetCount: n);
                        break;
                    }
                case ToeplitzMatrixType.Nonperiodic:
                    {
                        if (y.Count > n) { y = y.Truncate(targetCount: n); }
                        if (y.Count < n) { y = y.Padding(targetCount: n); }
                        break;
                    }
                default:
                    goto case ToeplitzMatrixType.Nonperiodic;
            }

            return y;
        }

        /// <summary>
        /// makes a toeplitz matrix for 1D case
        /// using the direct rule
        /// </summary>
        /// <param name="f"> input epsilon or mu in spatial-domain </param>
        /// <param name="nKxs"> number of spatial frequencies of fields, used to define truncation </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> result toeplitz matrix </returns>
        internal static MatrixZ ToeplitzMatrix(VectorZ f, long nKxs, ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            VectorZ y = new(other: f, deepCopy: true);
            
            y = toeplitztype switch
            {
                ToeplitzMatrixType.Periodic => ToeplitzValue(input: y, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Periodic),
                ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: y, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                _ => ToeplitzValue(input: y, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
            };

            return MakeToeplitzMatrix(input: y);
        }

        /// <summary>
        /// makes a toeplitz-block matrix for 2D case
        /// using the direct rule along x and y
        /// </summary>
        /// <param name="f"> input epsilon or mu in spatial domain </param>
        /// <param name="nKxs"> number of spatial frequencies of fields along x, used to define truncation </param>
        /// <param name="nKys"> number of spatial frequencies of fields along y, used to define truncation </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> result toeplitz-block matrix </returns>
        internal static MatrixZ DirXDirY(MatrixZ f, long nKxs, long nKys, ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            // sizes
            long n = nKxs * nKys; //nx * ny;
            MatrixZ btpz = new(n, n, 0.0);

            // data containers
            LongRange allRows = new(0, f.Rows);
            MatrixZ[] tpzy = new MatrixZ[f.Cols];

            // loop for x
            for (long i = 0; i < f.Cols; i++)
            {
                VectorZ vy = f[allRows, i];
                Transform.FFS1D(x: ref vy, isForward: true);
                vy = toeplitztype switch
                {
                    ToeplitzMatrixType.Periodic => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Periodic),
                    ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                    _ => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                };
                tpzy[i] = MakeToeplitzMatrix(vy);
            }

            // loop for ky and ky'
            for (long iyRow = 0; iyRow < nKys; iyRow++)
            {
                for (long iyCol = 0; iyCol < nKys; iyCol++)
                {
                    VectorZ vx = new(f.Cols, 0.0);
                    for (long i = 0; i < f.Cols; i++) { vx[i, false] = tpzy[i][iyRow, iyCol, false]; }
                    Transform.FFS1D(x: ref vx, isForward: true);
                    vx = toeplitztype switch
                    {
                        ToeplitzMatrixType.Periodic => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Periodic),
                        ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                        _ => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                    };
                    MatrixZ tpzx = MakeToeplitzMatrix(vx);

                    // fill into the final block toeplitz matrix
                    // loop for kx and kx'
                    for (long ixRow = 0; ixRow < nKxs; ixRow++)
                    {
                        for (long ixCol = 0; ixCol < nKxs; ixCol++)
                        {
                            btpz[iyRow * nKxs + ixRow, iyCol * nKxs + ixCol, false] = tpzx[ixRow, ixCol, false];
                        }
                    }
                }
            }

            // return
            return btpz;
        }

        /// <summary>
        /// makes a toeplitz-block matrix for 2D case
        /// using the inverse rule along x and direct rule along y
        /// </summary>
        /// <param name="f"> input epsilon or mu in spatial domain </param>
        /// <param name="nKxs"> number of spatial frequencies of fields along x, used to define truncation </param>
        /// <param name="nKys"> number of spatial frequencies of fields along y, used to define truncation </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> result toeplitz-block matrix </returns>
        internal static MatrixZ InvXDirY(MatrixZ f, long nKxs, long nKys, ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            // sizes
            long n = nKxs * nKys;
            MatrixZ btpz = new(n, n, 0.0);

            // data containers
            LongRange allCols = new(0, f.Cols);
            MatrixZ[] tpzx = new MatrixZ[f.Rows];

            // loop for y
            for (long i = 0; i < f.Rows; i++)
            {
                VectorZ vx = 1.0 / f[i, allCols];
                Transform.FFS1D(x: ref vx, isForward: true);
                vx = toeplitztype switch
                {
                    ToeplitzMatrixType.Periodic => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Periodic),
                    ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                    _ => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                };
                tpzx[i] = MakeToeplitzMatrix(vx);
                LinAlg.Inverse(ref tpzx[i]);
            }

            // loop for kx and kx'
            for (long ixRow = 0; ixRow < nKxs; ixRow++)
            {
                for (long ixCol = 0; ixCol < nKxs; ixCol++)
                {
                    VectorZ vy = new(f.Rows, 0.0);
                    for (long i = 0; i < f.Rows; i++) { vy[i, false] = tpzx[i][ixRow, ixCol, false]; }
                    Transform.FFS1D(x: ref vy, isForward: true);
                    vy = toeplitztype switch
                    {
                        ToeplitzMatrixType.Periodic => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Periodic),
                        ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                        _ => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                    };
                    MatrixZ tpzy = MakeToeplitzMatrix(vy);

                    // fill into the final block toeplitz matrix
                    // loop for ky and ky'
                    for (long iyRow = 0; iyRow < nKys; iyRow++)
                    {
                        for (long iyCol = 0; iyCol < nKys; iyCol++)
                        {
                            btpz[iyRow * nKxs + ixRow, iyCol * nKxs + ixCol, false] = tpzy[iyRow, iyCol, false];
                        }
                    }
                }
            }

            // return
            return btpz;
        }

        /// <summary>
        /// makes a toeplitz-block matrix for 2D case
        /// using the inverse rule along x and y
        /// </summary>
        /// <param name="f"> input epsilon or mu in spatial domain </param>
        /// <param name="nKxs"> number of spatial frequencies of fields along x, used to define truncation </param>
        /// <param name="nKys"> number of spatial frequencies of fields along y, used to define truncation </param>
        /// <param name="toeplitztype">Type of Toeplitz matrix (Periodic or Nonperiodic).</param>
        /// <returns> result toeplitz-block matrix </returns>
        internal static MatrixZ InvYDirX(MatrixZ f, long nKxs, long nKys, ToeplitzMatrixType toeplitztype = ToeplitzMatrixType.Nonperiodic)
        {
            // sizes
            long n = nKxs * nKys;
            MatrixZ btpz = new(n, n, 0.0);

            // data containers
            LongRange allRows = new(0, f.Rows);
            MatrixZ[] tpzy = new MatrixZ[f.Cols];

            // loop for x
            for (long i = 0; i < f.Cols; i++)
            {
                VectorZ vy = 1.0 / f[allRows, i];
                Transform.FFS1D(x: ref vy, isForward: true);
                vy = toeplitztype switch
                {
                    ToeplitzMatrixType.Periodic => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Periodic),
                    ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                    _ => ToeplitzValue(input: vy, nKxs: nKys, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                };
                tpzy[i] = MakeToeplitzMatrix(vy);
                LinAlg.Inverse(ref tpzy[i]);
            }

            // loop for ky and ky'
            for (long iyRow = 0; iyRow < nKys; iyRow++)
            {
                for (long iyCol = 0; iyCol < nKys; iyCol++)
                {
                    VectorZ vx = new(f.Cols, 0.0);
                    for (long i = 0; i < f.Cols; i++) { vx[i, false] = tpzy[i][iyRow, iyCol, false]; }
                    Transform.FFS1D(x: ref vx, isForward: true);
                    vx = toeplitztype switch
                    {
                        ToeplitzMatrixType.Periodic => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Periodic),
                        ToeplitzMatrixType.Nonperiodic => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                        _ => ToeplitzValue(input: vx, nKxs: nKxs, toeplitztype: ToeplitzMatrixType.Nonperiodic),
                    };
                    MatrixZ tpzx = MakeToeplitzMatrix(vx);

                    // fill into the final block toeplitz matrix
                    // loop for kx and kx'
                    for (long ixRow = 0; ixRow < nKxs; ixRow++)
                    {
                        for (long ixCol = 0; ixCol < nKxs; ixCol++)
                        {
                            btpz[iyRow * nKxs + ixRow, iyCol * nKxs + ixCol, false] = tpzx[ixRow, ixCol, false];
                        }
                    }
                }
            }

            // return
            return btpz;
        }

        #endregion
    }
}
