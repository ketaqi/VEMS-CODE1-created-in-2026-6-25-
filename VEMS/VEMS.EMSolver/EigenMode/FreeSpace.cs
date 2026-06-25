using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    ///// <summary>
    ///// eigen-info calculation for 
    ///// homogeneous isotropic free space
    ///// </summary>
    //[Obsolete("use UniformLayer instead")]
    //public class FreeSpace
    //{

    //    // static computation methods ...
    //    #region ------- eigenvalues -------

    //    /// <summary>
    //    /// computes nz i.e. kz/k0 normalized
    //    /// for scalar kx and ky
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity </param>
    //    /// <param name="mu"> complex permeability </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="ny"> normalized ky (ny = ky/k0) </param>
    //    /// <param name="scal"> additional scaling factor </param>
    //    /// <returns> scalar nz = (scal*) kz/k0 </returns>
    //    public static Complex ComputeNz(Complex epsilon, Complex mu,
    //        double nx, double ny = 0.0,
    //        double scal = 1.0)
    //        => scal * Complex.Sqrt(epsilon * mu - nx * nx - ny * ny);

    //    /// <summary>
    //    /// computes nz i.e. kz/k0 normalized
    //    /// for vectorized kx and scalar ky
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity </param>
    //    /// <param name="mu"> complex permeability </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="ny"> normalized ky (ny = ky/k0) </param>
    //    /// <param name="scal"> additional scaling factor </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> vector nz = (scal*) kz/k0 </returns>
    //    public static VectorZ ComputeNz(Complex epsilon, Complex mu,
    //        VectorD nx, double ny = 0.0,
    //        double scal = 1.0,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        Complex emp = epsilon * mu;
    //        VectorZ nz = new(nx.Count);
    //        double ny2 = ny * ny;

    //        // loop mode switch
    //        switch (loopMode)
    //        {
    //            case LoopMode.Sequential:
    //                {
    //                    for (long i = 0; i < nz.Count; i++)
    //                    {
    //                        double inx = nx[i, false];
    //                        nz[i, false] = scal * Complex.Sqrt(emp - inx * inx - ny2);
    //                    }
    //                    break;
    //                }
    //            case LoopMode.Parallel:
    //                {
    //                    Parallel.For(0, nz.Count, i =>
    //                    {
    //                        double inx = nx[i, false];
    //                        nz[i, false] = scal * Complex.Sqrt(emp - inx * inx - ny2);
    //                    });
    //                    break;
    //                }
    //            case LoopMode.Vectorized:
    //                {
    //                    nz = VMath.Sqrt(emp - ny2 - VMath.Square(nx));
    //                    if (scal != 1.0) { VMath.ScaleOn(ref nz, scal); }
    //                    break;
    //                }
    //            default: goto case LoopMode.Parallel;
    //        }
    //        // return
    //        return nz;
    //    }

    //    /// <summary>
    //    /// computes nz i.e. kz/k0 normalized
    //    /// for vectorized kx and vectorized ky
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity </param>
    //    /// <param name="mu"> complex permeability </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="ny"> normalized ky (ny = ky/k0) </param>
    //    /// <param name="scal"> additional scaling factor </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> matrix nz = (scal*) kz/k0 </returns>
    //    public static MatrixZ ComputeNz(Complex epsilon, Complex mu,
    //        VectorD nx, VectorD ny,
    //        double scal = 1.0,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        Complex emp = epsilon * mu;
    //        MatrixZ nz = new(ny.Count, nx.Count);

    //        // loop mode switch
    //        switch (loopMode)
    //        {
    //            case LoopMode.Sequential:
    //                {
    //                    for (long i = 0; i < nz.Rows * nz.Cols; i++)
    //                    {
    //                        long iRow = i / nz.Cols;
    //                        long iCol = i % nz.Cols;
    //                        double iny = ny[iRow, false];
    //                        double inx = nx[iCol, false];
    //                        double iny2 = iny * iny;
    //                        double inx2 = inx * inx;
    //                        nz[iRow, iCol, false] = scal * Complex.Sqrt(emp - inx2 - iny2);
    //                    }
    //                    break;
    //                }
    //            case LoopMode.Parallel:
    //                {
    //                    Parallel.For(0, nz.Rows * nz.Cols, i =>
    //                    {
    //                        long iRow = i / nz.Cols;
    //                        long iCol = i % nz.Cols;
    //                        double iny = ny[iRow, false];
    //                        double inx = nx[iCol, false];
    //                        double iny2 = iny * iny;
    //                        double inx2 = inx * inx;
    //                        nz[iRow, iCol, false] = scal * Complex.Sqrt(emp - inx2 - iny2);
    //                    });
    //                    break;
    //                }
    //            case LoopMode.Vectorized:
    //                {
    //                    VectorD nx2 = VMath.Square(nx);
    //                    VectorD ny2 = VMath.Square(ny);
    //                    MatrixD nx2m = LinAlg.GenerateRowMatrix(nx2, nz.Rows);
    //                    MatrixD ny2m = LinAlg.GenerateRowMatrix(ny2, nz.Cols);
    //                    nz = VMath.Sqrt(emp - nx2m - ny2m);
    //                    if (scal != 1.0) { VMath.ScaleOn(ref nz, scal); }
    //                    break;
    //                }
    //            default: goto case LoopMode.Parallel;
    //        }
    //        // return
    //        return nz;
    //    }

    //    #endregion
    //    #region ------- eigenmodes -------

    //    #region ===== in-plane [scalar kx] =====

    //    /// <summary>
    //    /// computes the eigen information for in-plane case
    //    /// for a selected polarization mode
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity @wavelength </param>
    //    /// <param name="mu"> complex permeability @wavelength </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="polarization"> in-plane polarization mode: TE or TM </param>
    //    /// <param name="direction"> direction of propagation </param>
    //    /// <returns> eigen-info (nz, w) </returns>
    //    public static (Complex, Complex) ComputeInPlaneModes(Complex epsilon, 
    //        Complex mu, double nx,
    //        InPlanePolMode polarization = InPlanePolMode.TE,
    //        SignFactor direction = SignFactor.Positive)
    //    {
    //        // computes nz first
    //        Complex nz = ComputeNz(epsilon, mu, nx,
    //            ny: 0.0, scal: (int)direction);
    //        // computes w
    //        Complex w = polarization switch
    //        {
    //            InPlanePolMode.TE => -nz / mu,
    //            InPlanePolMode.TM => epsilon / nz,
    //            _ => -nz / mu
    //        };
    //        return (nz, w);
    //    }

    //    /// <summary>
    //    /// computes the eigen information for in-plane case
    //    /// for both TE and TM polarization modes
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity @wavelength </param>
    //    /// <param name="mu"> complex permeability @wavelength </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="direction"> direction of propagation </param>
    //    /// <returns> eigen-info (nz, wC, wD) </returns>
    //    public static (Complex, Complex, Complex) ComputeInPlaneModes(Complex epsilon, 
    //        Complex mu, double nx,
    //        SignFactor direction = SignFactor.Positive)
    //    {
    //        // computes nz
    //        Complex nz = ComputeNz(epsilon, mu, nx,
    //            ny: 0.0, scal: (int)direction);
    //        // compute wc and wd
    //        Complex wc = epsilon / nz; // TM
    //        Complex wd = -nz / mu; // TE
    //        return (nz, wc, wd);
    //    }

    //    #endregion
    //    #region ===== in-plane [vector kx] =====

    //    /// <summary>
    //    /// computes the eigen information for in-plane case
    //    /// for a selected polarization mode
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity @wavelength </param>
    //    /// <param name="mu"> complex permeability @wavelength </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="polarization"> in-plane polarization mode: TE or TM </param>
    //    /// <param name="direction"> direction of propagation </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> eigen-info (nz, w) </returns>
    //    public static (VectorZ, VectorZ) ComputeEigen(Complex epsilon, 
    //        Complex mu, VectorD nx,
    //        InPlanePolMode polarization = InPlanePolMode.TE,
    //        SignFactor direction = SignFactor.Positive,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        // computes nz
    //        VectorZ nz = ComputeNz(epsilon, mu, nx,
    //            ny: 0.0, scal: (int)direction, loopMode: loopMode);
    //        // computes w
    //        VectorZ w = new(nz.Count);
    //        switch(loopMode)
    //        {
    //            case LoopMode.Sequential:
    //                {
    //                    for(long i = 0; i < nz.Count; i++)
    //                    {
    //                        w[i, false] = polarization switch
    //                        {
    //                            InPlanePolMode.TE => -nz[i, false] / mu,
    //                            InPlanePolMode.TM => epsilon / nz[i, false],
    //                            _ => -nz[i, false] / mu
    //                        };
    //                    }
    //                    break;
    //                }
    //            case LoopMode.Parallel:
    //                {
    //                    Parallel.For(0, nz.Count, i =>
    //                    {
    //                        w[i, false] = polarization switch
    //                        {
    //                            InPlanePolMode.TE => -nz[i, false] / mu,
    //                            InPlanePolMode.TM => epsilon / nz[i, false],
    //                            _ => -nz[i, false] / mu
    //                        };
    //                    });
    //                    break;
    //                }
    //            case LoopMode.Vectorized:
    //                {
    //                    w = polarization switch
    //                    {
    //                        InPlanePolMode.TE => (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu),
    //                        InPlanePolMode.TM => epsilon / nz,
    //                        _ => (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu)
    //                    };
    //                    break;
    //                }
    //            default: goto case LoopMode.Sequential;
    //        }
    //        // return
    //        return (nz, w);
    //    }

    //    /// <summary>
    //    /// computes the eigen information for in-plane case
    //    /// for both TE and TM polarization modes
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity @wavelength </param>
    //    /// <param name="mu"> complex permeability @wavelength </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="direction"> direction of propagation </param>
    //    /// <param name="loopMode"> loop-computational mode options </param>
    //    /// <returns> eigen-info (nz, wC, wD) </returns>
    //    public static (VectorZ, VectorZ, VectorZ) ComputeEigen(Complex epsilon, 
    //        Complex mu, VectorD nx,
    //        SignFactor direction = SignFactor.Positive,
    //        LoopMode loopMode = Defaults.LoopOption)
    //    {
    //        // computes nz
    //        VectorZ nz = ComputeNz(epsilon, mu, nx,
    //            ny: 0.0, scal: (int)direction, loopMode: loopMode);
    //        // compute wc and wd
    //        VectorZ wc = new(nz.Count);
    //        VectorZ wd = new(nz.Count);
    //        switch(loopMode)
    //        {
    //            case LoopMode.Sequential:
    //                {
    //                    for (long i = 0; i < nz.Count; i++)
    //                    {
    //                        wc[i, false] = epsilon / nz[i, false];
    //                        wd[i, false] = -nz[i, false] / mu;
    //                    }
    //                    break;
    //                }
    //            case LoopMode.Parallel:
    //                {
    //                    Parallel.For(0, nz.Count, i =>
    //                    {
    //                        wc[i, false] = epsilon / nz[i, false];
    //                        wd[i, false] = -nz[i, false] / mu;
    //                    });
    //                    break;
    //                }
    //            case LoopMode.Vectorized:
    //                {
    //                    wc = epsilon / nz;  // TM
    //                    wd = (mu == 1.0) ? -nz : VMath.Scale(nz, -1.0 / mu);
    //                    break;
    //                }
    //            default: goto case LoopMode.Sequential;
    //        }
    //        // return
    //        return (nz, wc, wd);
    //    }

    //    #endregion
    //    #region ===== conical [scalar (kx,ky)] =====

    //    /// <summary>
    //    /// computes the eigen information for conical case
    //    /// </summary>
    //    /// <param name="epsilon"> complex permittivity @wavelength </param>
    //    /// <param name="mu"> complex permeability @wavelength </param>
    //    /// <param name="nx"> normalized kx (nx = kx/k0) </param>
    //    /// <param name="ny"> normalized ky (ny = ky/k0) </param>
    //    /// <param name="direction"> direction of propagation </param>
    //    /// <returns> eigen-info (nz, wB, wC, wD) </returns>
    //    public static (Complex, Complex, Complex, Complex) ComputeConicalModes(
    //        Complex epsilon, Complex mu, 
    //        double nx, double ny,
    //        SignFactor direction = SignFactor.Positive)
    //    {
    //        // computes nz first
    //        Complex nz = ComputeNz(epsilon, mu, nx,
    //            ny: 0.0, scal: (int)direction);
    //        // computes w
    //        Complex wb = nx * ny / (mu * nz);
    //        Complex wc = nz / mu + nx * nx / (mu * nz);
    //        Complex wd = nz / mu + ny * ny / (mu * nz);
    //        // return
    //        return (nz, wb, wc, wd);
    //    }

    //    #endregion
    //    #region ===== conical [vector ...] =====

    //    // ...

    //    #endregion

    //    #endregion

    //}
}
