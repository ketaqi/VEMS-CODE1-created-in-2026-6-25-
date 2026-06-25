namespace VEMS.MathCore
{

    /// <summary>
    /// collection of commonly used functions in optics
    /// </summary>
    public class CommonFunction
    {
        #region ------- Factorial -------

        /// <summary>
        /// computes factorial of a non-negative integer
        /// </summary>
        /// <param name="n"> input non-negative integer </param>
        /// <returns> factorial </returns>
        public static int Factorial(int n)
        {
            if (n == 0) { return 1; }
            int f = n;
            while (n > 1)
            {
                n--;
                f *= n;
            }
            return f;
        }

        #endregion
        #region ------- Zernike -------

        /// <summary>
        /// radial Zernike polynomai
        /// </summary>
        public class ZernikeR
        {
            #region properties

            /// <summary>
            /// lower index
            /// </summary>
            public int N { get; set; }

            /// <summary>
            /// upper index
            /// </summary>
            public int M { get; set; }

            /// <summary>
            /// polynomial coefficients for Rnm
            /// </summary>
            public int[] C { get; set; }

            /// <summary>
            /// normalization factor
            /// </summary>
            public double A { get; set; }

            #endregion
            #region constructors

            /// <summary>
            /// default constructor
            /// </summary>
            internal ZernikeR() { C = new int[1]; }

            /// <summary>
            /// constructs a radial Zernike polynomial
            /// for given indices n and m
            /// </summary>
            /// <param name="n"> lower index </param>
            /// <param name="m"> upper index </param>
            public ZernikeR(int n, int m)
            {
                N = n;
                M = m;
                // polynomial coefficients
                C = Function1D.CoefficientsRnm(N, M);
                //// normalization factor
                //double dm0 = (m == 0) ? 1.0 : 0.0;
                //A = Math.Sqrt(2.0 * (N + 1) / (1.0 + dm0));
            }

            #endregion
            #region methods

            /// <summary>
            /// evaluate value of R_n^m at radial position
            /// </summary>
            /// <param name="rho"> radial variable between 0 and 1 </param>
            /// <returns> value of R_n^m(\rho) </returns>
            public double Evaluate(double rho)
            {
                if (rho < 0.0 || rho > 1.0) { return 0.0; }
                else { return Function1D.Polyn(rho, C.ToList()); }
            }

            #endregion
        }

        /// <summary>
        /// Zernike polynomial
        /// </summary>
        public class Zernike : ZernikeR
        {

            #region properties

            ///// <summary>
            ///// type of Zernike polynomials: even or odd
            ///// </summary>
            //public ZernikeType Type { get; set; }

            /// <summary>
            /// single-indexing convention
            /// </summary>
            public ZernikeIndexing Indexing { get; set; }

            ///// <summary>
            ///// single-index
            ///// </summary>
            //public int Idx { get; set; }

            ///// <summary>
            ///// single-index according to Noll
            ///// </summary>
            //public int IdxNoll { get; set; }

            ///// <summary>
            ///// single-index according to OSA/ANSI standard
            ///// </summary>
            //public int IdxStd { get; set; }

            ///// <summary>
            ///// single-index according to Fringe/University of Arizona
            ///// </summary>
            //public int IdxFringe { get; set; }

            /// <summary>
            /// radial part of the Zernike polynomial
            /// </summary>
            public ZernikeR RadialPart { get; set; }

            /// <summary>
            /// azimuthal part of the Zernike polynomial
            /// sin(m*Phi) or cos(m*Phi)
            /// </summary>
            public Func<double, double> AzimuthalPart { get; set; }

            /// <summary>
            /// reference radius in the polar coordinate system
            /// </summary>
            public double RefRadius { get; set; } 

            /// <summary>
            /// normalization factor in different Zernike polynomials
            /// </summary>
            public double NormalPart { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs a Zernike polynomial with given indices
            /// </summary>
            /// <param name="n"> index n </param>
            /// <param name="m"> index m </param>
            /// <param name="indexing"> Indexing mode of Zernike polynomials </param>
            /// <param name="refRadius"> refernece radius in the polar coordinate system </param>
            public Zernike (int n, int m, ZernikeIndexing indexing, double refRadius = 1.0) //:base(n, m)
            {
                // defines radial part
                RadialPart = new(n, Math.Abs(m));
                Indexing = indexing;
                // defines azimuthal part
                if (Indexing == ZernikeIndexing.Noll)
                {
                    Printer.Warning($"Noll indexing isn't supported.");
                    throw new ArgumentException();
                }
                else
                {
                    if ((n - Math.Abs(m)) < 0 || (n - Math.Abs(m)) % 2 != 0)
                    {
                        Printer.Warning($"n-|m|≥0 and must even.");
                        throw new ArgumentException();
                    }
                    else
                    {
                        if (m < 0)
                        {
                            AzimuthalPart = (phi) => Math.Sin(Math.Abs(m) * phi);
                        }
                        else
                        {
                            AzimuthalPart = (phi) => Math.Cos(Math.Abs(m) * phi);
                        }
                    }
                    RefRadius = refRadius;
                    // defines normalization factor
                    NormalPart = Math.Sqrt((2.0 * (n + 1)) / (1.0 + (m == 0 ? 1 : 0)));
                }
            }

            /// <summary>
            /// constructs a Zernike polynomial with given single index and indexing mode 
            /// </summary>
            /// <param name="idx"> single-index of Zernike polynomials Idx </param>
            /// <param name="indexing"> Indexing mode of Zernike polynomials </param>
            /// <param name="refRadius"> reference radius in the polar coordinate system </param>
            public Zernike (int idx, ZernikeIndexing indexing, 
                double refRadius = 1.0)
            {
                Indexing = indexing;
                (int n, int m) = IndexCalculationNM(idx, Indexing);
                // defines radial part
                RadialPart = new(n, Math.Abs(m));
                // defines azimuthal part
                if (m < 0)
                {
                    AzimuthalPart = (phi) => Math.Sin(Math.Abs(m) * phi);
                }
                else
                {
                    AzimuthalPart = (phi) => Math.Cos(Math.Abs(m) * phi);
                }
                RefRadius = refRadius;
                // defines normalization factor
                NormalPart = Math.Sqrt((2.0 * (n + 1)) / (1.0 + (m == 0 ? 1 : 0)));
            }

            #endregion
            #region methods

            /// <summary>
            /// convert from index to number
            /// </summary>
            /// <param name="idx"> single-index of Zernike polynomials Idx </param>
            /// <param name="indexing"> Indexing mode of Zernike polynomials </param>
            /// <returns> number of Zps </returns>
            private static int IndexCalculationJ(int idx, ZernikeIndexing indexing)
            {
                switch (indexing)
                {
                    case ZernikeIndexing.Noll:
                        return idx;
                    case ZernikeIndexing.Standard:
                        return (idx + 1);
                    case ZernikeIndexing.Fringe:
                        if (idx > 36)
                        {
                            // idx must be less than or equal to 36
                            Printer.Warning($"Index must less than or equal to 36");
                            throw new Exception("Index must less than or equal to 36");
                        }
                        else
                        {
                            return (idx + 1);
                        }
                    case ZernikeIndexing.ExtendFringe:
                        return (idx + 1);
                    case ZernikeIndexing.BornWolf:
                        return idx;
                }
                return 0;
            }

            /// <summary>
            /// calculates number of Zernike polynomials according to 
            /// the given indexing mode and single index
            /// </summary>
            /// <param name="idx"> single-index of Zernike polynomials Idx </param>
            /// <returns> number of Zernike polynomials </returns>
            private int IndexCalculationJ(int idx)
                => Indexing switch
                {
                    ZernikeIndexing.Noll => idx,
                    ZernikeIndexing.BornWolf => idx,
                    ZernikeIndexing.Standard => (idx + 1),
                    ZernikeIndexing.ExtendFringe => (idx + 1),
                    ZernikeIndexing.Fringe => idx > 36 ? 
                    throw new ArgumentOutOfRangeException("Index must less than or equal to 36") : (idx + 1),
                    _ => (idx + 1)
                };

            /// <summary>
            /// claculate the Zernike polynomials index according to the given indexing mode
            /// </summary>
            /// <param name="idx"> index of Zernike polynomials Idx</param>
            /// <param name="indexing"> Indexing mode of Zernike polynomials </param>
            /// <returns> lower index 'n' and upper index 'm'</returns>
            private (int, int) IndexCalculationNM(int idx, ZernikeIndexing indexing)
            {
                // convert int Idx into double j
                double j = (double)IndexCalculationJ(idx, indexing);
                double[] boundary = new double[4];
                // calculate the n of Zernike polynomials(calculate Fringe/ExtendedFringe N to n)
                if (indexing == ZernikeIndexing.Fringe || indexing == ZernikeIndexing.ExtendFringe)
                {
                    boundary[0] = Math.Ceiling(-1.0 * Math.Sqrt(j));
                    boundary[1] = Math.Floor(+1.0 * Math.Sqrt(j));
                    boundary[2] = Math.Ceiling(+1.0 * Math.Sqrt(j) - 1.0);
                    boundary[3] = Math.Floor(-1.0 * Math.Sqrt(j) - 1.0);
                }
                else
                {
                    // Standard, Noll, BornWolf index have the same calculation in n
                    boundary[0] = Math.Ceiling(-1.0 * Math.Sqrt(2.0 * j + 1.0 / 4.0) - 1.0 / 2.0);
                    boundary[1] = Math.Floor(+1.0 * Math.Sqrt(2.0 * j + 1.0 / 4.0) - 1.0 / 2.0);
                    boundary[2] = Math.Floor(-1.0 * Math.Sqrt(2.0 * j + 1.0 / 4.0) - 3.0 / 2.0);
                    boundary[3] = Math.Ceiling(+1.0 * Math.Sqrt(2.0 * j + 1.0 / 4.0) - 3.0 / 2.0);
                }
                // filter the minimum value of boundary that is not negative -> n
                double n_min = boundary.Where(n => n >= 0).Min();
                // n_min double to int
                int n = n_min >= 0 ? (int)n_min : default(int);
                int m = 0;
                int jInt = (int)j;
                // calculate the m of Zernike polynomials to the given indexing mode
                switch(Indexing)
                {
                    case ZernikeIndexing.Noll:
                        {
                            if (n % 2 == 0)
                            {
                                if ((jInt - 1 == (1 + n) * n / 2))
                                { m = 0; }
                                else
                                { m = (jInt - (1 + n) * n / 2) / 2 * 2; }
                            }
                            else
                            {
                                if ((jInt - (1 + n) * n / 2) % 2 != 0)
                                { m = jInt - (1 + n) * n / 2; }
                                else
                                { m = jInt - (1 + n) * n / 2 - 1; }
                            }
                            // Noll to Standard
                            if (jInt % 2 == 0)
                            { m = +1 * m; }
                            else
                            { m = -1 * m; }
                            break;
                        }
                    case ZernikeIndexing.Standard:
                        {
                            if (jInt - 1 == (1 + n) * n / 2)
                            { m = -1 * n; }
                            else if (jInt - 1 > (1 + n) * n / 2)
                            { m = -1 * n + 2 * (jInt - (1 + n) * n / 2 - 1); }
                            break;
                        }
                    case ZernikeIndexing.Fringe:
                        {
                            int N = n;
                            if (N == 6)
                            { n = 2 * N; m = 0; }
                            else
                            {
                                if ((jInt - 1 - (N - 1) * (N - 1)) % 2 == 0)
                                {
                                    n = (jInt - 1 - (N - 1) * (N - 1)) / 2;
                                    m = n - 2 * N;
                                }
                                else
                                {
                                    n = (jInt - (N - 1) * (N - 1)) / 2;
                                    m = 2 * N - n;
                                }    
                            }
                            break;
                        }

                    case ZernikeIndexing.ExtendFringe:
                        {
                            // N of ExtendFringe is twice as much as N of Fringe
                            int N = n * 2;
                            // calculated n by N and jInt
                            if ((jInt - 1 - (N / 2 - 1) * (N / 2 - 1)) % 2 == 0)
                            {
                                n = (jInt - 1 - (N / 2 - 1) * (N / 2 - 1)) / 2;
                                m = n - N;
                            }
                            else
                            {
                                n = (jInt - (N / 2 - 1) * (N / 2 - 1)) / 2;
                                m = N - n;
                            }
                            break;
                        }
                    case ZernikeIndexing.BornWolf:
                        {
                            if (jInt - 1 == (1 + n) * n / 2)
                            { m = n; }
                            else if (jInt - 1 > (1 + n) * n / 2)
                            { m = n - 2 * (jInt - (1 + n) * n / 2 - 1); }
                            break;
                        }
                }
                return (n, m);              
            }


            // private ...

            /// <summary>
            /// evaluates the Zernike polynomial at given (rho, phi)
            /// </summary>
            /// <param name="rho"> radial variable between 0 and 1</param>
            /// <param name="phi"> azimuthal variable between 0 and 2*PI </param>
            /// <returns></returns>
            public double Evaluate(double rho, double phi)
            {
                if (Indexing == ZernikeIndexing.Noll || Indexing == ZernikeIndexing.Standard)
                {  
                    return RadialPart.Evaluate(rho / RefRadius) * AzimuthalPart.Invoke(phi) * NormalPart;
                }
                else
                {
                    return RadialPart.Evaluate(rho / RefRadius) * AzimuthalPart.Invoke(phi);
                }
            }

            /// <summary>
            /// samples the function on a given uniform grid
            /// </summary>
            /// <param name="grid"> target grid </param>
            /// <param name="loopMode"> loop-computational mode options </param>
            /// <returns> sampled function values on the grid </returns>
            /// <exception cref="NotImplementedException"></exception>
            public MatrixD SampleOnGrid(GridInfo2D grid,
                LoopMode loopMode = LoopMode.Parallel)
            {
                MatrixD f = new(grid.Rows, grid.Cols);
                Action<long, long> action = (iRow, iCol) =>
                {
                    double y = grid.GetCoordinateY(iRow);
                    double x = grid.GetCoordinateX(iCol);
                    // convert to polar
                    var rho = Math.Sqrt(x * x + y * y);
                    var phi = Math.Atan2(y, x);
                    f[iRow, iCol, false] = Evaluate(rho, phi);
                };
                // loopMode choose
                Loop2D loop = new(operation: action, rowStart: 0, rowEnd: grid.Rows, 
                                                     colStart: 0, colEnd: grid.Cols,
                                                     rowStep: 1, colStep: 1);
                loop.Evaluate(loopMode);
                return f;
            }

            #endregion
            #region static methods


            public static int? NM2J(int n, int m,
                ZernikeIndexing indexing)
            {
                // null case definition
                int mm = Math.Abs(m);
                if(n < 0 || n < mm || (n-mm)%2 != 0) { return null; }
                // different indexing
                switch(indexing)
                {
                    case ZernikeIndexing.Noll: // one-based indexing
                        {
                            // number of terms for (n-1)
                            int j = n * (n + 1) / 2;
                            if (n%2 == 0) // even n
                            { j += 1 + (mm / 2) * 2; }
                            else // odd n
                            { j += (mm / 2 + 1) * 2; }
                            if (m > 0) { j -= 1; } // even term
                            return j;
                        }
                    case ZernikeIndexing.Standard: // zero-based indexing
                        {
                            // number of terms for (n-1)
                            int j = n * (n + 1) / 2;
                            // ascending (m) from -n till m
                            j += (m + n) / 2;
                            return j;
                        }
                    case ZernikeIndexing.Fringe: // zero-based indexing
                        {
                            int N = (n + mm) / 2;
                            // number of terms for (N-1)
                            int j = N * N;
                            // ascending n = 2N - |m|
                            // ...
                            // descending from n till m
                            // ...
                        }
                        break;
                    case ZernikeIndexing.ExtendFringe:
                        break;
                    case ZernikeIndexing.BornWolf:
                        break;
                    default: goto case ZernikeIndexing.ExtendFringe;
                }

                return 0;
            }

            private static (int, int) J2NM(int j)
            {
                return (0, 0);
            }


            #endregion
        }

        ///// <summary>
        ///// type of Zernike polynomials
        ///// </summary>
        //public enum ZernikeType
        //{
        //    /// <summary>
        //    /// even Zernike polynomial
        //    /// </summary>
        //    Even,

        //    /// <summary>
        //    /// odd Zernike polynomial
        //    /// </summary>
        //    Odd
        //}

        /// <summary>
        /// single indexing convention
        /// </summary>
        public enum ZernikeIndexing
        {
            /// <summary>
            /// Noll's sequential indices, Zemax Standard Zps
            /// </summary>
            Noll,

            /// <summary>
            /// OSA/ANSI standard indices, COMSOL Ray Optics Module
            /// </summary>
            Standard,

            /// <summary>
            /// Fringe/University of Arizona indices, CODE V, Zemax, OLSO
            /// </summary>
            Fringe,

            /// <summary>
            /// Extended Fringe/CODE V Extended Fringe Zps, ISO-14999
            /// </summary>
            ExtendFringe,

            /// <summary>
            /// Born and Wolf/CODE V Standard ZPs 
            /// </summary>
            BornWolf,
        }

        #endregion
    }

}
