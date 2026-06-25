using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// helper methods for RCWA
    /// </summary>
    public class RCWAHelper
    {
        #region ==== sampling ==== 

        /// <summary>
        /// determines sampling number by setting  
        /// a target resolution in x-domain
        /// </summary>
        /// <param name="period"> period in x-domain </param>
        /// <param name="dx"> target resolution in x-domain </param>
        /// <returns> total (odd) number of sampling </returns>
        public static long DetermineSampling(double period, double dx)
        {
            // reaches target resolution in x-domain
            long n = (long)Math.Ceiling(period / dx);
            // ensure odd number
            if (n % 2 == 0) { n += 1; }
            // return 
            return n;
        }

        /// <summary>
        /// determines sampling number by setting 
        /// target window size in k-domain
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="period"> period in x-domain </param>
        /// <param name="kSizeFactor"> k-domain size factor w.r.t. k0 </param>
        /// <param name="addNum"> (possibly) additional number </param>
        /// <returns> total (odd) sampling number of sampling </returns>
        public static long DetermineSampling(double wavelength,
            double period,
            double kSizeFactor = 1.5,
            long addNum = 0)
        {
            // single-side sampling numbers
            long half = (long)Math.Ceiling(kSizeFactor * period / wavelength);
            // double the number and add the center spatial frequency
            return 2 * (half + addNum) + 1; // odd number ensured
        }

        /// <summary>
        /// generates 1D uniformly distributed transverse
        /// spatial frequencies in a vector
        /// </summary>
        /// <param name="n"> number of total spatial frequencies </param>
        /// <param name="dk"> uniform distance in k-domain </param>
        /// <param name="kc"> center shift </param>
        /// <returns> spatial frequency vector </returns>
        public static VectorD GenerateKs(long n, double dk,
            double kc = 0.0)
        {
            // consistency check
            if (n % 2 == 0)
            { throw new ArgumentException($"n is supposed to be odd ..."); }
            // generate k vector
            VectorD vk = new(count: n,
                initVal: -(n - 1) / 2 * dk + kc,
                increment: dk);
            return vk;
        }

        #endregion
        #region ==== convertions ====

        /// <summary>
        /// converts (Ex, Ey) from 2D format into 1D format
        /// following the row-major convention
        /// for RCWA calculation
        /// </summary>
        /// <param name="fieldEx"> Ex field stored in 2D format </param>
        /// <param name="fieldEy"> Ey field stored in 2D format </param>
        /// <returns> result vector </returns>
        /// <exception cref="Exception"></exception>
        public static VectorZ Convert2X2DFieldTo1D(MatrixZ fieldEx, MatrixZ fieldEy)
        {
            if (fieldEx.Rows != fieldEy.Rows || fieldEx.Cols != fieldEy.Cols)
            { throw new Exception($"Unequal Field Matrix Sizes"); }

            long nRow = fieldEx.Rows;
            long nCol = fieldEx.Cols;
            long n = nRow * nCol;

            VectorZ v = new(count: 2 * n);
            LongRange allCols = new(start: 0, end: nCol);
            for (long iRow = 0; iRow < nRow; iRow++)
            {
                v[new LongRange(iRow * nCol, (iRow + 1) * nCol)] = fieldEx[iRow, allCols];
                v[new LongRange(n + iRow * nCol, n + (iRow + 1) * nCol)] = fieldEy[iRow, allCols];
            }

            return v;
        }

        /// <summary>
        /// converts (Ex, Ey) from 1D format back to 2D format
        /// following the row-major convention
        /// after RCWA calculation
        /// </summary>
        /// <param name="v"> (Ex, Ey) fields stored in 1D format </param>
        /// <param name="nRow"> number of rows in target 2D format </param>
        /// <param name="nCol"> number of columns in target 2D format </param>
        /// <returns> result two matrices (Ex, Ey) </returns>
        /// <exception cref="Exception"></exception>
        public static (MatrixZ, MatrixZ) Convert1DFieldTo2X2D(VectorZ v,
            long nRow, long nCol)
        {
            if (2 * nRow * nCol != v.Count)
            { throw new Exception("Vector Count and Nx Ny does not match"); }

            long n = nRow * nCol;
            MatrixZ Ex = new(rows: nRow, cols: nCol);
            MatrixZ Ey = new(rows: nRow, cols: nCol);
            LongRange allCols = new(start: 0, end: nCol);
            for (long iRow = 0; iRow < nRow; iRow++)
            {
                Ex[iRow, allCols] = v[new LongRange(iRow * nCol, (iRow + 1) * nCol)];
                Ey[iRow, allCols] = v[new LongRange(n + iRow * nCol, n + (iRow + 1) * nCol)];
            }

            return (Ex, Ey);
        }

        #endregion
        #region ==== plane wave ====

        /// <summary>
        /// defines the coefficients vector for a plane wave
        /// with its spatial frequency centered in the vector
        /// and scales its amplitude according to the period
        /// </summary>
        /// <param name="pw"> plane wave </param>
        /// <param name="period"> period </param>
        /// <param name="n"> total [odd] number of spatial frequencies </param>
        /// <returns> coefficients vector and its grid information </returns>
        public static (VectorZ, GridInfo1D) PlaneWaveToCoefficients(PlaneWaveXZ pw,
            double period,
            long n)
        {
            VectorZ c = new(count: n);
            // scaling factor
            double scal = period / Math.Sqrt(2.0 * Math.PI);
            // fills the value in vector
            c[(n - 1) / 2, checkBound: false] = scal * pw.E;
            // grid information
            double dKx = 2.0 * Math.PI / period;
            GridInfo1D g = new(n: n, spacing: dKx,
                start: pw.Kx - (n - 1) / 2 * dKx);
            return (c, g);
        }

        /// <summary>
        /// defines the coefficients matrices for a plane wave
        /// with its spatial frequency centered in the matrices
        /// and scales its amplitude according to the periods
        /// </summary>
        /// <param name="pw"> plane wave </param>
        /// <param name="periodX"> period along the x direction </param>
        /// <param name="periodY"> period along the y direction </param>
        /// <param name="nx"> total [odd] number of spatial frequencies along x </param>
        /// <param name="ny"> total [odd] number of spatial frequencies along y </param>
        /// <returns> coefficients matrices and the grid information </returns>
        public static (MatrixZ, MatrixZ, GridInfo2D) PlaneWaveToCoefficients(PlaneWave pw,
            double periodX, double periodY,
            long nx, long ny)
        {
            MatrixZ cEx = new(rows: ny, cols: nx);
            MatrixZ cEy = new(rows: ny, cols: nx);
            // scaling factor
            double scal = periodX * periodY / (2.0 * Math.PI);
            // fills the value in matrix
            cEx[(ny - 1) / 2, (nx - 1) / 2, checkBound: false] = scal * pw.Ex;
            cEy[(ny - 1) / 2, (nx - 1) / 2, checkBound: false] = scal * pw.Ey;
            // grid information
            double dKx = 2.0 * Math.PI / periodX;
            double dKy = 2.0 * Math.PI / periodY;
            GridInfo2D g = new(rows: ny, cols: nx, 
                spacingY: dKy, spacingX: dKx,
                startY: pw.Ky - (ny - 1) / 2 * dKy,
                startX: pw.Kx - (nx - 1) / 2 * dKx);
            return (cEx, cEy, g);
        }

        /// <summary>
        /// converts a mode coefficient to a plane wave
        /// </summary>
        /// <param name="c"> mode coefficient </param>
        /// <param name="period"> period of the grating </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> permittivity @wavelength </param>
        /// <param name="mu"> permeabilty @wavelength </param>
        /// <param name="kx"> spatial frrquency kx </param>
        /// <param name="polarization"> in-plane polarization option </param>
        /// <param name="direction"> direction of the plane wave </param>
        /// <returns> resulting plane wave with adjusted amplitude </returns>
        public static PlaneWaveXZ CoefficientToPlaneWave(Complex c, double period,
            double wavelength, Complex epsilon, Complex mu,
            double kx, InPlanePolMode polarization,
            SignFactor direction = SignFactor.Positive)
        {
            // makes a plane wave
            PlaneWaveXZ pw = new(wavelength: wavelength,
                epsilon: epsilon, mu: mu, kx: kx,
                direction: direction, polMode: polarization,
                initializeEigenInfo: true);
            // defines amplitude according to coefficients and period
            double scal = Math.Sqrt(2.0 * Math.PI) / period;
            pw.E = scal * c;
            // return
            return pw;
        }

        /// <summary>
        /// converts mode coefficients to a plane wave
        /// </summary>
        /// <param name="cEx"> mode coefficient for Ex-field </param>
        /// <param name="cEy"> mode coefficient for Ey-field </param>
        /// <param name="periodX"> period of the grating along x direction </param>
        /// <param name="periodY"> period of the grating along y direction </param>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> permittivity @wavelength </param>
        /// <param name="mu"> permeability @wavelength </param>
        /// <param name="kx"> spatial frequency along x direction </param>
        /// <param name="ky"> spatial frequency along y direction </param>
        /// <param name="direction"> direction of the plane wave </param>
        /// <returns> resulting plane wave with adjusted amplitudes </returns>
        public static PlaneWave CoefficientToPlaneWave(Complex cEx, Complex cEy,
            double periodX, double periodY, 
            double wavelength, Complex epsilon, Complex mu,
            double kx, double ky, 
            SignFactor direction = SignFactor.Positive)
        {
            // makes a plane wave
            PlaneWave pw = new(wavelength: wavelength,
                epsilon: epsilon, mu: mu, kx: kx, ky: ky,
                direction: direction,
                initializeEigenInfo: true);
            // defines amplitudes according to coefficients and period
            double scal = 2.0 * Math.PI / (periodX * periodY);
            pw.Ex = scal * cEx;
            pw.Ey = scal * cEy;
            // return
            return pw;
        }

        #endregion
    }
}
