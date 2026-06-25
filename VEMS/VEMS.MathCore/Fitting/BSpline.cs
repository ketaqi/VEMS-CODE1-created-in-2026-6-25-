using WMathCore;

namespace VEMS.MathCore
{

    /// <summary>
    /// collection of common methods for BSpline
    /// </summary>
    public class BSpline
    {
        #region ---- Knots ----

        /// <summary>
        /// types of b-spline knots
        /// </summary>
        public enum KnotsType
        {
            /// <summary>
            /// uniform knots 
            /// </summary>
            Uniform,

            /// <summary>
            /// clamped knots
            /// </summary>
            Clamped,

            /// <summary>
            /// todo ...
            /// </summary>
            Periodic
        }

        /// <summary>
        /// generates knots for b-spline fitting
        /// </summary>
        /// <param name="us"> sample locations </param>
        /// <param name="p"> degree of the basis function, p = 0, 1, 2 </param>
        /// <param name="nFactor"> factor between 0.0 and 1.0 for defining the number of knots spans </param>
        /// <param name="type"> knots type option </param>
        /// <returns> knots generated for b-spline fitting </returns>
        public static VectorD GenerateKnots(VectorD us,
            long p = 3, double nFactor = 0.7,
            KnotsType type = KnotsType.Clamped)
        {
            // nFactor range handling
            if (nFactor < 0.0)
            {
                nFactor = 0.0;
                Printer.Warning("nFactor automatically set to 0.0");
            }
            if (nFactor > 1.0)
            {
                nFactor = 1.0;
                Printer.Warning("nFactor automatically set to 1.0");
            }

            // u-sample boundaries
            double uMin = us[0];
            double uMax = us[^1];
            // define u-sample range division ... from experience factor
            long nDiv = (long)Math.Ceiling(nFactor * us.Count);

            // exceptions
            if (nDiv <= 0) { nDiv = 1; }
            if (nDiv + 2 * p >= us.Count) { nDiv = us.Count - 2 * p; }

            // compute uniform knot span size
            double du = (uMax - uMin) / nDiv;
            // define knot spans
            long spans = nDiv + 2 * p;

            // define uniform knots
            VectorD knots = new (count: spans + 1,
                initVal: uMin - p * du,
                increment: du);
            switch (type)
            {
                case KnotsType.Uniform:
                    break;
                case KnotsType.Clamped:
                    {
                        for (long i = 0; i < p + 1; i++)
                            knots[i] = uMin;
                        for (long i = spans - p + 1; i < spans + 1; i++)
                            knots[i] = uMax;
                        break;
                    }
                default: goto case KnotsType.Clamped;
            }
            return knots;
        }

        /// <summary>
        /// binary search for the non-zero knot span 
        /// for b-spline basis function at 0th degree
        /// </summary>
        /// <param name="knots"> knots u_0, u_1, ... u_m </param>
        /// <param name="u"> variable u </param>
        /// <param name="k"> [out] k-th knot span containing the non-zero basis function </param>
        /// <returns> whether the non-zere span is found or not </returns>
        [Obsolete("use Search.Binary instead")]
        public static bool NonZeroSpanSearch(VectorD knots, double u, long p,
            out long k)
        {
            // initialize
            k = 0;

            // invalid cases
            if (knots.Count < 2)
                return false;

            // general cases
            //if (u < knots[0 + p] || u > knots[knots.Count - 1 - p])
            //    return false;
            if (u < knots[0] || u > knots[knots.Count - 1])
                return false;
            else if (knots.Count == 2)
                return true;
            else
            {
                long iBegin = 0 + p;
                long iEnd = knots.Count - 1 - p;

                while (iEnd - iBegin != 1)
                {
                    long iMid = iBegin + (iEnd - iBegin) / 2;
                    // within [iBegin, iMid) ??
                    if (u >= knots[iBegin] && u < knots[iMid])
                        iEnd = iMid;
                    else
                        iBegin = iMid;
                }
                k = iBegin;

                return true;
            }

        }

        #endregion
        #region ---- Basis Function ----

        /// <summary>
        /// computes only the non-zero n-matrix elements in a row
        /// </summary>
        /// <param name="k"> index of the non-zero knot span for p = 0 </param>
        /// <param name="p"> degree of spline polynomial </param>
        /// <param name="u"> variable within the k-th knot span </param>
        /// <param name="knots"> knots vector </param>
        /// <returns> non-zero row elements </returns>
        public static VectorD NRow(long k, long p, double u, VectorD knots)
        {
            // initialization
            VectorD nRow = new(p + 1);
            nRow[p, false] = 1.0;

            // loop for higher degree p
            for (long d = 1; d <= p; d++)
            {
                #region first [i = k-d]

                double duFirst = knots[k + 1, false] - knots[k - d + 1, false];
                nRow[p - d, false] = (knots[k + 1, false] - u) / duFirst * nRow[p - d + 1, false];

                #endregion
                #region inner loop [k-d+1 <= i <= k-1]

                for (long i = k - d + 1; i < k; i++)
                {
                    double duLeft = knots[i + d, false] - knots[i, false];
                    double duRight = knots[i + d + 1, false] - knots[i + 1, false];

                    long ii = i - (k - d); // convert to local index system
                    nRow[p - d + ii, false] = (u - knots[i, false]) / duLeft * nRow[p - d + ii, false]
                        + (knots[i + d + 1, false] - u) / duRight * nRow[p - d + ii + 1, false];
                }

                #endregion
                #region last [i = k]

                double duLast = knots[k + d, false] - knots[k, false];
                nRow[p, false] = (u - knots[k, false]) / duLast * nRow[p, false];

                #endregion
            }

            return nRow;
        }

        /// <summary>
        /// computes only the non-zero n-matrix elements in a row
        /// and store the results in a sparse vector 
        /// </summary>
        /// <param name="k"> index of the non-zero knot span for p = 0 </param>
        /// <param name="p"> degree of spline polynomial </param>
        /// <param name="u"> variable within the k-th knot span </param>
        /// <param name="knots"> knots vector </param>
        /// <returns> non-zero row elements stored in compressed format </returns>
        public static WVectorDi NRowi(long k, long p, double u, VectorD knots)
        {
            // call NRow method directly
            VectorD nzv = NRow(k, p, u, knots);

            // initialize sparse vector
            WVectorDi nRow = new(knots.Count - 1 - p, p + 1);
            // !!!
            //VectorI nzi = new(p + 1, k - p, 1);
            VectorI nzi = new(p + 1);
            // !!!
            Sparse.FillWVectorDi(ref nRow, nzi, nzv);

            return nRow;
        }

        /// <summary>
        /// computes only the non-zero element in the n-matrix 
        /// and stores the results in the compressed format
        /// </summary>
        /// <param name="knots"> knots locations {u_0, u_1, ..., u_m} </param>
        /// <param name="p"> degree of the basis function, p = 0, 1, 2 </param>
        /// <param name="us"> sample locations </param>
        /// <returns> n-matrix consists of the b-spline basis functions </returns>
        public static WMatrixDi NMatrixi(VectorD knots, long p, VectorD us)
        {
            // initialize NW for sparse matrix
            WMatrixDi n = Sparse.InitWMatrixDi(us.Count,
                knots.Count - 1 - p,
                (p + 1) * us.Count);
            VectorI rowPtr = new(us.Count + 1);
            rowPtr[0] = 0;
            VectorI colIdx = new((p + 1) * us.Count);
            VectorD nzCSR = new((p + 1) * us.Count);

            // loop over rows
            for (long iRow = 0; iRow < us.Count; iRow++)
            {
                double u = us[iRow];
                // find the non-zero knot span for p = 0
                bool spanFound = NonZeroSpanSearch(knots, u, p, out long k);
                if (!spanFound)
                {
                    Printer.Write($"Non-zero span not found at row #{iRow} with u={u}");
                    break;
                }
                // call NRow method
                VectorD nRow = NRow(k, p, u, knots);

                rowPtr[iRow + 1] = (iRow + 1) * (p + 1);
                for (long j = 0; j <= p; j++)
                {
                    colIdx[iRow * (p + 1) + j] = k - p + j;
                    nzCSR[iRow * (p + 1) + j] = nRow[j];
                }
            }

            Sparse.FillWMatrixDiCSR(ref n, rowPtr, colIdx, nzCSR);

            return n;
        }

        #endregion

        // updates ...
        #region ---- Knots ----

        /// <summary>
        /// 
        /// </summary>
        /// <param name="us"></param>
        /// <param name="p"></param>
        /// <param name="nFactor"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Vect<double> GenerateKnots(Vect<double> us,
            long p = 3, double nFactor = 0.7,
            KnotsType type = KnotsType.Clamped)
        {
            // nFactor range handling
            if (nFactor < 0.0)
            {
                nFactor = 0.0;
                Printer.Warning("nFactor automatically set to 0.0");
            }
            if (nFactor > 1.0)
            {
                nFactor = 1.0;
                Printer.Warning("nFactor automatically set to 1.0");
            }

            // u-sample boundaries
            double uMin = us[0, false];
            double uMax = us[us.Count - 1, false];
            // define u-sample range division ... from experience factor
            long nDiv = (long)Math.Ceiling(nFactor * us.Count);

            // exceptions
            if (nDiv <= 0) { nDiv = 1; }
            if (nDiv + 2 * p >= us.Count) { nDiv = us.Count - 2 * p; }

            // compute uniform knot span size
            double du = (uMax - uMin) / nDiv;
            // define knot spans
            long spans = nDiv + 2 * p;

            // define uniform knots
            Vect<double> knots = Vect<double>.Create(
                n: spans + 1,
                x0: uMin - p * du, 
                dx: du);
            switch (type)
            {
                case KnotsType.Uniform:
                    break;
                case KnotsType.Clamped:
                    {
                        for (long i = 0; i < p + 1; i++)
                        { knots[i] = uMin; }
                        for (long i = spans - p + 1; i < spans + 1; i++)
                        { knots[i] = uMax; }
                        break;
                    }
                case KnotsType.Periodic:
                    throw new NotImplementedException("Periodic knots are not implemented yet.");
                default: goto case KnotsType.Clamped;
            }
            return knots;
        }


        #endregion
        #region ---- Basis Function ----

        /// <summary>
        /// 
        /// </summary>
        /// <param name="k"></param>
        /// <param name="p"></param>
        /// <param name="u"></param>
        /// <param name="knots"></param>
        /// <returns></returns>
        private static Vect<double> NRowKernel(long k, long p, double u, 
            VectorD knots)
        {
            // initialization
            Vect<double> nRow = new(count: p + 1, 
                initMode: ArrayInitMode.Malloc);
            nRow[p, false] = 1.0;

            // loop for higher degree p
            for (long d = 1; d <= p; d++)
            {
                // first [i = k-d]
                double duFirst = knots[k + 1, false] - knots[k - d + 1, false];
                double nRowPrev = nRow[p - d + 1, false];
                nRow[p - d, false] = duFirst != 0.0
                    ? (knots[k + 1, false] - u) / duFirst * nRowPrev
                    : 0.0;

                // inner loop [k-d+1 <= i <= k-1]
                for (long i = k - d + 1; i < k; i++)
                {
                    long ii = i - (k - d);

                    double duLeft = knots[i + d, false] - knots[i, false];
                    double duRight = knots[i + d + 1, false] - knots[i + 1, false];

                    double left = duLeft != 0.0
                        ? (u - knots[i, false]) / duLeft * nRow[p - d + ii, false]
                        : 0.0;
                    double right = duRight != 0.0
                        ? (knots[i + d + 1, false] - u) / duRight * nRow[p - d + ii + 1, false]
                        : 0.0;

                    nRow[p - d + ii, false] = left + right;
                }

                // last [i = k]
                double duLast = knots[k + d, false] - knots[k, false];
                nRow[p, false] = duLast != 0.0
                    ? (u - knots[k, false]) / duLast * nRow[p, false]
                    : 0.0;
            }

            return nRow;
        }


        public static void NMatx(VectorD knots, long p, VectorD us)
        {
            // initializes sparse info
            Vect<long> rowPtr = new (count: us.Count + 1, initMode: ArrayInitMode.Malloc);
            rowPtr[0, false] = 0;
            long nnz = (p + 1) * us.Count;
            Vect<long> colIdx = new (count: nnz, initMode: ArrayInitMode.Malloc);
            Vect<double> nzCSR = new(count: nnz, initMode: ArrayInitMode.Malloc);

            // loop over rows
            for (long iRow = 0; iRow < us.Count; iRow++)
            {
                double u = us[iRow, false];
                // find the non-zero knot span for p = 0
                bool spanFound = Search.Binary(xs: knots, x: u, out long k);
                //bool spanFound = NonZeroSpanSearch(knots, u, p, out long k);
                if (!spanFound)
                {
                    Printer.Write($"Non-zero span not found at row #{iRow} with u={u}");
                    break;
                }
                // call NRow method
                Vect<double> nRow = NRowKernel(k, p, u, knots);

                rowPtr[iRow + 1, false] = (iRow + 1) * (p + 1);
                for (long j = 0; j <= p; j++)
                {
                    colIdx[iRow * (p + 1) + j, false] = k - p + j;
                    nzCSR[iRow * (p + 1) + j, false] = nRow[j, false];
                }
            }

            // assemble the sparse matrix
            //SPMatrix<double> n = new SPMatrix<double>(
            //    rows: us.Count, cols: knots.Count - 1 - p,
            //    nnz: nnz,
            //    rowPtr: rowPtr, colIdx: colIdx, nzVal: nzCSR);

            //WMatrixDi n = Sparse.InitWMatrixDi(us.Count,
            //    knots.Count - 1 - p,
            //    (p + 1) * us.Count);
            //Sparse.FillWMatrixDiCSR(ref n, rowPtr, colIdx, nzCSR);

            //return n;
        }

        #endregion

        #region ... obsolete ...

        /// <summary>
        /// computes one row [in dense format] in the n-matrix 
        /// </summary>
        /// <param name="p"> degree of spline polynomial </param>
        /// <param name="u"> variable within the k-th knot span </param>
        /// <param name="knots"> knots vector </param>
        /// <returns> full row stored in dense format </returns>
        [Obsolete("use the other NRow method or NRowi method instead")]
        public static VectorD NRow(long p, double u, VectorD knots)
        {
            // initialize
            VectorD nRowFull = new(knots.Count - 1 - p);

            // find the none-zero knot span for p = 0
            // with binary search
            bool spanFound = NonZeroSpanSearch(knots, u, p, out long k);
            if (spanFound == false) // special case handling
            {
                Printer.Write("Non-zero span not found");
                return nRowFull;
            }

            // call NRow method to find those non-zero elements
            VectorD nRow = NRow(k, p, u, knots);
            // fill in the full dense vector
            LongRange rng = new(k - p, k + 1);
            nRowFull[rng] = nRow;

            return nRowFull;
        }

        /// <summary>
        /// computes the n-matrix which consists of 
        /// the bspline basis functions 
        /// </summary>
        /// <param name="knots"> knots locations {u_0, u_1, ..., u_m} </param>
        /// <param name="p"> degree of the basis function, p = 0, 1, 2 </param>
        /// <param name="us"> sample locations </param>
        /// <returns> n-matrix consists of the b-spline basis functions </returns>
        [Obsolete]
        public static MatrixD NMatrix(VectorD knots, long p, VectorD us)
        {
            // initialize
            MatrixD n = new(us.Count, knots.Count - 1 - p);

            // loop over rows
            for (long iRow = 0; iRow < n.Rows; iRow++)
            {
                double u = us[iRow];
                // find the non-zero knot span for p = 0
                bool spanFound = NonZeroSpanSearch(knots, u, p, out long k);
                if (!spanFound)
                {
                    Printer.Write($"Non-zero span not found at row #{iRow}");
                    break;
                }
                // call NRow method
                VectorD nRow = NRow(k, p, u, knots);
                // fill into the matrix
                LongRange rng = new(k - p, k + 1);
                n[iRow, rng] = nRow;
            }

            return n;
        }

        #endregion

    }



}
