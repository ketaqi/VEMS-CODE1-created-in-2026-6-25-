using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Debye diffraction integral
    /// </summary>
    public class Debye
    {

        /// <summary>
        /// Debye diffraction integral kernel (focusing by idealized lens)
        /// for an off-axis plane wave, with circular aperture at the pupil
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> refractive index @wavelength </param>
        /// <param name="diameter"> pupil diameter (circular shape) </param>
        /// <param name="focalLength"> focal length </param>
        /// <param name="distance"> actual propagation distance from pupil plane </param>
        /// <param name="targetGrid"> target sampling grid (defined in target domain) </param>
        /// <param name="targetDomain"> target modeling domain of the result </param>
        /// <param name="kx0"> x-component of the incident wavevector </param>
        /// <param name="ky0"> y-component of the incident wavevector </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> diffracted field at given distance in the target domain </returns>
        public static MatrixZ PropagateKernel(double wavelength, double n,
            double diameter, double focalLength,
            double distance,
            GridInfo2D targetGrid,
            ModelingDomain targetDomain = ModelingDomain.Spatial,
            double kx0 = 0.0, double ky0 = 0.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // parameters
            double k = n* 2.0 * Math.PI / wavelength;
            double k2 = k * k;

            // defines k-grid in the spatial frequency domain
            if (targetDomain == ModelingDomain.Spatial)
            { targetGrid.GetConjugated(isForward: true); }

            // computes result field in the spatial frequency domain
            MatrixZ v = new(targetGrid.Rows, targetGrid.Cols);
            
            // loop mode switch
            switch(loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for(long i = 0; i < v.Rows * v.Cols; i++)
                        {
                            long iRow = i / v.Cols;
                            long iCol = i % v.Cols;

                            double ky = targetGrid.GetCoordinateY(iRow);
                            double kx = targetGrid.GetCoordinateX(iCol);
                            double ky2 = ky * ky;
                            double kyp = ky - ky0;
                            double kyp2 = kyp * kyp;
                            double kx2 = kx * kx;
                            double kxp = kx - kx0;
                            double kxp2 = kxp * kxp;

                            double kz2 = k2 - kx2 - ky2;
                            double kzp2 = k2 - kxp2 - kyp2;

                            if (kz2 <= 0.0) // evanescent wave
                                v[iRow, iCol, false] = 0.0;
                            else
                            {
                                double kz = Math.Sqrt(kz2);
                                double kzp = Math.Sqrt(kzp2);

                                double rho = Math.Sqrt(kxp2 + kyp2) / kzp * focalLength;
                                if (rho <= 0.5 * diameter)
                                    v[iRow, iCol, false] = -Complex.ImaginaryOne * k * focalLength / kzp2
                                        * Complex.Exp(Complex.ImaginaryOne * (kz * distance - kzp * focalLength));
                                else // outside aperture
                                    v[iRow, iCol, false] = 0.0;
                            }
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, v.Rows * v.Cols, i =>
                        {
                            long iRow = i / v.Cols;
                            long iCol = i % v.Cols;

                            double ky = targetGrid.GetCoordinateY(iRow);
                            double kx = targetGrid.GetCoordinateX(iCol);
                            double ky2 = ky * ky;
                            double kyp = ky - ky0;
                            double kyp2 = kyp * kyp;
                            double kx2 = kx * kx;
                            double kxp = kx - kx0;
                            double kxp2 = kxp * kxp;

                            double kz2 = k2 - kx2 - ky2;
                            double kzp2 = k2 - kxp2 - kyp2;

                            if (kz2 <= 0.0) // evanescent wave
                                v[iRow, iCol, false] = 0.0;
                            else
                            {
                                double kz = Math.Sqrt(kz2);
                                double kzp = Math.Sqrt(kzp2);

                                double rho = Math.Sqrt(kxp2 + kyp2) / kzp * focalLength;
                                if (rho <= 0.5 * diameter)
                                    v[iRow, iCol, false] = -Complex.ImaginaryOne * k * focalLength / kzp2
                                        * Complex.Exp(Complex.ImaginaryOne * (kz * distance - kzp * focalLength));
                                else // outside aperture
                                    v[iRow, iCol, false] = 0.0;
                            }
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }

            // handling output domain
            if (targetDomain == ModelingDomain.Spatial)
            { Transform.FFT2D(x: ref v, grid: ref targetGrid,
                    direction: FFTOptions.Direction.Backward); }

            // return
            return v;
        }

        /// <summary>
        /// Debye diffraction integral kernel (focusing by idealized lens)
        /// for an off-axis plane wave, with arbitrary aperture at the pupil
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> refractive index @wavelength </param>
        /// <param name="aperture"> sampled aperture at the pupil </param>
        /// <param name="focalLength"> focal length </param>
        /// <param name="distance"> actual propagation distance from pupil plane </param>
        /// <param name="targetGrid"> target sampling grid (defined in target domain) </param>
        /// <param name="targetDomain"> target modeling domain of the result </param>
        /// <param name="kx0"> x-component of the incident wavevector </param>
        /// <param name="ky0"> y-component of the incident wavevector </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> diffracted field at given distance in the target domain </returns>
        public static MatrixZ PropagateKernel(double wavelength, double n,
            Grid2DRealData aperture, double focalLength, double distance,
            GridInfo2D targetGrid, 
            ModelingDomain targetDomain = ModelingDomain.Spatial,
            double kx0 = 0.0, double ky0 = 0.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // parameters
            double k = n * 2.0 * Math.PI / wavelength;
            double k2 = k * k;

            // defines k-grid in the spatial frequency domain
            if (targetDomain == ModelingDomain.Spatial)
            { targetGrid.GetConjugated(isForward: true); }

            // compute result field in the spatial frequency domain
            MatrixZ v = new(targetGrid.Rows, targetGrid.Cols);

            // loop mode switch
            switch(loopMode)
            {
                case LoopMode.Sequential:
                    {
                        for(long i = 0; i < v.Rows * v.Cols; i++)
                        {
                            long iRow = i / v.Cols;
                            long iCol = i % v.Cols;

                            double ky = targetGrid.GetCoordinateY(iRow);
                            double kx = targetGrid.GetCoordinateX(iCol);
                            double ky2 = ky * ky;
                            double kyp = ky - ky0;
                            double kyp2 = kyp * kyp;
                            double kx2 = kx * kx;
                            double kxp = kx - kx0;
                            double kxp2 = kxp * kxp;

                            double kz2 = k2 - kx2 - ky2;
                            double kzp2 = k2 - kxp2 - kyp2;

                            if (kz2 <= 0.0) // evanescent wave
                                v[iRow, iCol, false] = 0.0;
                            else
                            {
                                double kz = Math.Sqrt(kz2);
                                double kzp = Math.Sqrt(kzp2);

                                // find spatial coordinates
                                double x = kxp / kzp * focalLength;
                                double y = kyp / kzp * focalLength;
                                // interpolate WHAT method to use???
                                double t = Interpolation.BiLinear(v: aperture.Values, grid: aperture.GridInfo, x, y);
                                v[iRow, iCol, false] = -t * Complex.ImaginaryOne * k * focalLength / kzp2
                                    * Complex.Exp(Complex.ImaginaryOne * (kz * distance - kzp * focalLength));
                            }
                        }
                        break;
                    }
                case LoopMode.Parallel:
                    {
                        Parallel.For(0, v.Rows * v.Cols, i =>
                        {
                            long iRow = i / v.Cols;
                            long iCol = i % v.Cols;

                            double ky = targetGrid.GetCoordinateY(iRow);
                            double kx = targetGrid.GetCoordinateX(iCol);
                            double ky2 = ky * ky;
                            double kyp = ky - ky0;
                            double kyp2 = kyp * kyp;
                            double kx2 = kx * kx;
                            double kxp = kx - kx0;
                            double kxp2 = kxp * kxp;

                            double kz2 = k2 - kx2 - ky2;
                            double kzp2 = k2 - kxp2 - kyp2;

                            if (kz2 <= 0.0) // evanescent wave
                                v[iRow, iCol, false] = 0.0;
                            else
                            {
                                double kz = Math.Sqrt(kz2);
                                double kzp = Math.Sqrt(kzp2);

                                // find spatial coordinates
                                double x = kxp / kzp * focalLength;
                                double y = kyp / kzp * focalLength;
                                // interpolate WHAT method to use???
                                double t = Interpolation.BiLinear(v: aperture.Values, grid: aperture.GridInfo, x, y);
                                v[iRow, iCol, false] = -t * Complex.ImaginaryOne * k * focalLength / kzp2
                                    * Complex.Exp(Complex.ImaginaryOne * (kz * distance - kzp * focalLength));
                            }
                        });
                        break;
                    }
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }

            // handling output domain
            if (targetDomain == ModelingDomain.Spatial)
            { Transform.FFT2D(x: ref v, grid: ref targetGrid, 
                direction: FFTOptions.Direction.Backward); }

            // return
            return v;
        }


    }
}
