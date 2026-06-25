using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using VEMS.MathCore;
using static VEMS.EMSolver.MaxwellODE;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Maxwell ODE for different cases
    /// </summary>
    public class MaxwellODE
    {
        /// <summary>
        /// Maxwell ODE for 2D isotopic cases
        /// </summary>
        public class MaxwellODE2D() 
        {
            #region properties
            /// <summary>
            /// medium function for z as input and Layer2DMedium as output
            /// </summary>
            public Layer2DMedium Medium { get; set; }

            /// <summary>
            ///  gridinfo in k-domain
            /// </summary>
            public GridInfo2D GridK { get; set; }

            /// <summary>
            /// gridinfo in x-domain
            /// </summary>
            public GridInfo2D GridX { get; set; }

            /// <summary>
            /// wavelength of the incident emfield
            /// </summary>
            public double Wavelength { get; set; }

            /// <summary>
            /// filter width in kx direction to get rid of evanescent wave
            /// </summary>
            public double FilterKx { get; set; }

            /// <summary>
            /// filter width in ky direction to get rid of evanescent wave
            /// </summary>
            public double FilterKy { get; set; }



            #endregion
            #region cunstructors
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="medium"> medium function </param>
            /// <param name="gridK"> gridinfo in k-domain </param>
            /// <param name="gridX"> gridinfo in x-domain </param>
            /// <param name="wavelength"> wavelength of the incident emfield </param>
            /// <param name="filterKx"> filter width in kx direction </param>
            /// <param name="filterKy"> filter width in ky direction </param>

            public MaxwellODE2D(Layer2DMedium medium,
                GridInfo2D gridK,
                GridInfo2D gridX,
                double wavelength,
                double filterKx,
                double filterKy) : this()
            {
                Medium = medium;
                GridK = gridK;
                GridX = gridX;
                Wavelength = wavelength;
                FilterKx = filterKx;
                FilterKy = filterKy;
            }

            #endregion
            #region methods
            /// <summary>
            ///  construct a 2D Maxwell ODE in k-domain
            /// </summary>
            /// <param name="z"> the coordinate of propagation direction </param>
            /// <param name="input"> the matrix containing Ex, Ey, Hx, and Hy </param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            internal MatrixZ dFz(MatrixZ input)
            {
                //caluculate k0
                double k0 = 2 * Math.PI / Wavelength;

                //turn input vector into Ex, Ey and Hx, Hy
                if (input.Rows % 4 != 0) { throw new ArgumentException(); }
                long n = input.Rows / 4;
                MatrixZ Ex = new MatrixZ(input[new LongRange(0, n), new LongRange(0, input.Cols)]);
                MatrixZ Ey = new MatrixZ(input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)]);
                MatrixZ Hx = new MatrixZ(input[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)]);
                MatrixZ Hy = new MatrixZ(input[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)]);

                //get epsilon and sampled
                MatrixZ epsilon = Medium.Sample(Wavelength, GridX, MaterialProperty.Epsilon);

                //parameters
                MatrixZ a = new MatrixZ(Ex);
                MatrixZ b = new MatrixZ(Ey);
                MatrixZ c = new MatrixZ(Hx);
                MatrixZ d = new MatrixZ(Hy);
                GridInfo2D gridK = GridK;
                
                //for mu equals 1.0 cases
                if(Medium.Mu == null)
                {
                    //convolutions by FFT
                    MatrixZ convEx = new MatrixZ(Ex);
                    MatrixZ convEy = new MatrixZ(Ey);
                    MatrixZ convHx = new MatrixZ(Hx);
                    MatrixZ convHy = new MatrixZ(Hy);

                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);
                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            convHx[iy, ix] *= ky / k0;
                            convHy[iy, ix] *= kx / k0;
                        });
                    });

                    Transform.FFT2D(ref convEx, ref gridK, FTOption.Backward);
                    convEx *= epsilon;
                    Transform.FFT2D(ref convEx, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convEy, ref gridK, FTOption.Backward);
                    convEy *= epsilon;
                    Transform.FFT2D(ref convEy, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHx, ref gridK, FTOption.Backward);
                    convHx /= epsilon;
                    Transform.FFT2D(ref convHx, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHy, ref gridK, FTOption.Backward);
                    convHy /= epsilon;
                    Transform.FFT2D(ref convHy, ref gridK, FTOption.Forward);

                    //Maxwell ODE
                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);

                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            a[iy, ix] = (kx * convHx[iy, ix] / k0 + Hy[iy, ix] - kx * convHy[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            b[iy, ix] = (-Hx[iy, ix] + ky * convHx[iy, ix] / k0 - ky * convHy[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            c[iy, ix] = (-kx * ky * Ex[iy, ix] / k0 / k0 - convEy[iy, ix]
                                        + kx * kx * Ey[iy, ix] / k0 / k0) * k0 * Complex.ImaginaryOne;
                            d[iy, ix] = (convEx[iy, ix] - ky * ky * Ex[iy, ix] / k0 / k0
                                        + kx * ky * Ey[iy, ix] / k0 / k0) * k0 * Complex.ImaginaryOne;
                            a[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            b[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            c[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            d[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                        });
                    });

                    //return the output
                    MatrixZ output = new MatrixZ(input.Rows, input.Cols);
                    output[new LongRange(0, n), new LongRange(0, input.Cols)] = a;
                    output[new LongRange(n, 2 * n), new LongRange(0, input.Cols)] = b;
                    output[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)] = c;
                    output[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)] = d;
                    return output;
                }
                // for mu inhomogeneous cases
                else
                {
                    MatrixZ mu = Medium.Sample(Wavelength, GridX, MaterialProperty.Mu);

                    //convolutions by FFT
                    MatrixZ convExe = new MatrixZ(Ex);
                    MatrixZ convEye = new MatrixZ(Ey);
                    MatrixZ convHxe = new MatrixZ(Hx);
                    MatrixZ convHye = new MatrixZ(Hy);
                    MatrixZ convExm = new MatrixZ(Ex);
                    MatrixZ convEym = new MatrixZ(Ey);
                    MatrixZ convHxm = new MatrixZ(Hx);
                    MatrixZ convHym = new MatrixZ(Hy);

                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);
                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            convExm[iy, ix] *= ky / k0;
                            convEym[iy, ix] *= kx / k0;
                            convHxe[iy, ix] *= ky / k0;
                            convHye[iy, ix] *= kx / k0;
                        });
                    });

                    Transform.FFT2D(ref convExe, ref gridK, FTOption.Backward);
                    convExe *= epsilon;
                    Transform.FFT2D(ref convExe, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convEye, ref gridK, FTOption.Backward);
                    convEye *= epsilon;
                    Transform.FFT2D(ref convEye, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHxe, ref gridK, FTOption.Backward);
                    convHxe /= epsilon;
                    Transform.FFT2D(ref convHxe, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHye, ref gridK, FTOption.Backward);
                    convHye /= epsilon;
                    Transform.FFT2D(ref convHye, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convExm, ref gridK, FTOption.Backward);
                    convExm /= mu;
                    Transform.FFT2D(ref convExm, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convEym, ref gridK, FTOption.Backward);
                    convEym /= mu;
                    Transform.FFT2D(ref convEym, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHxm, ref gridK, FTOption.Backward);
                    convHxm *= mu;
                    Transform.FFT2D(ref convHxm, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHym, ref gridK, FTOption.Backward);
                    convHym *= mu;
                    Transform.FFT2D(ref convHym, ref gridK, FTOption.Forward);

                    //Maxwell ODE
                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);

                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            a[iy, ix] = (kx * convHxe[iy, ix] / k0 + convHym[iy, ix] - kx * convHye[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            b[iy, ix] = (-convHxm[iy, ix] + ky * convHxe[iy, ix] / k0 - ky * convHye[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            c[iy, ix] = (-kx * convExm[iy, ix] / k0 - convEye[iy, ix] + kx * convEym[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            d[iy, ix] = (convExe[iy, ix] - ky * convExm[iy, ix] / k0 + ky * convEym[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            a[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            b[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            c[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            d[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                        });
                    });

                    //return the output
                    MatrixZ output = new MatrixZ(input.Rows, input.Cols);
                    output[new LongRange(0, n), new LongRange(0, input.Cols)] = a;
                    output[new LongRange(n, 2 * n), new LongRange(0, input.Cols)] = b;
                    output[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)] = c;
                    output[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)] = d;
                    return output;

                }

            }

            /// <summary>
            /// Maxwell ODE with timespan
            /// </summary>
            /// <param name="input"> input field </param>
            /// <param name="calculating"> timespan for the whole calculation </param>
            /// <param name="converting"> timespan for converting between 4 field matrix and 1 output matrix </param>
            /// <param name="convolution"> timespan for convolution </param>
            /// <param name="multiplication"> time span for matrix multiplication </param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            internal MatrixZ dFz(MatrixZ input,
            ref TimeSpan calculating,
            ref TimeSpan converting,
            ref TimeSpan convolution,
            ref TimeSpan multiplication)
            {
                Stopwatch cal = Stopwatch.StartNew();
                cal.Start();
                // Calculate k0
                double k0 = 2 * Math.PI / Wavelength;

                Stopwatch convert = Stopwatch.StartNew();
                convert.Start();
                // Turn input matrix into Ex, Ey, Hx, and Hy
                if (input.Rows % 4 != 0) { throw new ArgumentException(); }
                long n = input.Rows / 4;
                MatrixZ Ex = new MatrixZ(input[new LongRange(0, n), new LongRange(0, input.Cols)]);
                MatrixZ Ey = new MatrixZ(input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)]);
                MatrixZ Hx = new MatrixZ(input[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)]);
                MatrixZ Hy = new MatrixZ(input[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)]);
                convert.Stop();

                // Get epsilon and sampled
                MatrixZ epsilon = Medium.Sample(Wavelength, GridX, MaterialProperty.Epsilon);

                // Parameters
                MatrixZ a = new MatrixZ(Ex);
                MatrixZ b = new MatrixZ(Ey);
                MatrixZ c = new MatrixZ(Hx);
                MatrixZ d = new MatrixZ(Hy);
                GridInfo2D gridK = GridK;

                // For mu equals 1.0 cases
                if (Medium.Mu == null)
                {
                    // Convolutions by FFT
                    Stopwatch convo = Stopwatch.StartNew();
                    convo.Start();
                    MatrixZ convEx = new MatrixZ(Ex);
                    MatrixZ convEy = new MatrixZ(Ey);
                    MatrixZ convHx = new MatrixZ(Hx);
                    MatrixZ convHy = new MatrixZ(Hy);

                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);
                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            convHx[iy, ix] *= ky / k0;
                            convHy[iy, ix] *= kx / k0;
                        });
                    });

                    Transform.FFT2D(ref convEx, ref gridK, FTOption.Backward);
                    convEx *= epsilon;
                    Transform.FFT2D(ref convEx, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convEy, ref gridK, FTOption.Backward);
                    convEy *= epsilon;
                    Transform.FFT2D(ref convEy, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHx, ref gridK, FTOption.Backward);
                    convHx /= epsilon;
                    Transform.FFT2D(ref convHx, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHy, ref gridK, FTOption.Backward);
                    convHy /= epsilon;
                    Transform.FFT2D(ref convHy, ref gridK, FTOption.Forward);
                    convo.Stop();
                    convolution += convo.Elapsed;

                    // Maxwell ODE
                    Stopwatch multi = Stopwatch.StartNew();
                    multi.Start();
                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);

                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            a[iy, ix] = (kx * convHx[iy, ix] / k0 + Hy[iy, ix] - kx * convHy[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            b[iy, ix] = (-Hx[iy, ix] + ky * convHx[iy, ix] / k0 - ky * convHy[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            c[iy, ix] = (-kx * ky * Ex[iy, ix] / k0 / k0 - convEy[iy, ix]
                                        + kx * kx * Ey[iy, ix] / k0 / k0) * k0 * Complex.ImaginaryOne;
                            d[iy, ix] = (convEx[iy, ix] - ky * ky * Ex[iy, ix] / k0 / k0
                                        + kx * ky * Ey[iy, ix] / k0 / k0) * k0 * Complex.ImaginaryOne;
                            a[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            b[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            c[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            d[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                        });
                    });
                    multi.Stop();
                    multiplication += multi.Elapsed;

                    // Return the output
                    convert.Start();
                    MatrixZ output = new MatrixZ(input.Rows, input.Cols);
                    output[new LongRange(0, n), new LongRange(0, input.Cols)] = a;
                    output[new LongRange(n, 2 * n), new LongRange(0, input.Cols)] = b;
                    output[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)] = c;
                    output[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)] = d;
                    convert.Stop();
                    converting += convert.Elapsed;
                    cal.Stop();
                    calculating += cal.Elapsed;

                    return output;
                }
                // For mu inhomogeneous cases
                else
                {
                    MatrixZ mu = Medium.Sample(Wavelength, GridX, MaterialProperty.Mu);

                    // Convolutions by FFT
                    Stopwatch convo = Stopwatch.StartNew();
                    convo.Start();
                    MatrixZ convExe = new MatrixZ(Ex);
                    MatrixZ convEye = new MatrixZ(Ey);
                    MatrixZ convHxe = new MatrixZ(Hx);
                    MatrixZ convHye = new MatrixZ(Hy);
                    MatrixZ convExm = new MatrixZ(Ex);
                    MatrixZ convEym = new MatrixZ(Ey);
                    MatrixZ convHxm = new MatrixZ(Hx);
                    MatrixZ convHym = new MatrixZ(Hy);

                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);
                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            convExm[iy, ix] *= ky / k0;
                            convEym[iy, ix] *= kx / k0;
                            convHxe[iy, ix] *= ky / k0;
                            convHye[iy, ix] *= kx / k0;
                        });
                    });

                    Transform.FFT2D(ref convExe, ref gridK, FTOption.Backward);
                    convExe *= epsilon;
                    Transform.FFT2D(ref convExe, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convEye, ref gridK, FTOption.Backward);
                    convEye *= epsilon;
                    Transform.FFT2D(ref convEye, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHxe, ref gridK, FTOption.Backward);
                    convHxe /= epsilon;
                    Transform.FFT2D(ref convHxe, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHye, ref gridK, FTOption.Backward);
                    convHye /= epsilon;
                    Transform.FFT2D(ref convHye, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convExm, ref gridK, FTOption.Backward);
                    convExm /= mu;
                    Transform.FFT2D(ref convExm, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convEym, ref gridK, FTOption.Backward);
                    convEym /= mu;
                    Transform.FFT2D(ref convEym, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHxm, ref gridK, FTOption.Backward);
                    convHxm *= mu;
                    Transform.FFT2D(ref convHxm, ref gridK, FTOption.Forward);
                    Transform.FFT2D(ref convHym, ref gridK, FTOption.Backward);
                    convHym *= mu;
                    Transform.FFT2D(ref convHym, ref gridK, FTOption.Forward);
                    convo.Stop();
                    convolution += convo.Elapsed;

                    // Maxwell ODE
                    Stopwatch multi = Stopwatch.StartNew();
                    multi.Start();
                    Parallel.For(0, gridK.Cols, ix =>
                    {
                        var kx = gridK.GetCoordinateX(ix);

                        Parallel.For(0, gridK.Rows, iy =>
                        {
                            var ky = gridK.GetCoordinateY(iy);
                            a[iy, ix] = (kx * convHxe[iy, ix] / k0 + convHym[iy, ix] - kx * convHye[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            b[iy, ix] = (-convHxm[iy, ix] + ky * convHxe[iy, ix] / k0 - ky * convHye[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            c[iy, ix] = (-kx * convExm[iy, ix] / k0 - convEye[iy, ix] + kx * convEym[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            d[iy, ix] = (convExe[iy, ix] - ky * convExm[iy, ix] / k0 + ky * convEym[iy, ix] / k0)
                                        * k0 * Complex.ImaginaryOne;
                            a[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            b[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            c[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                            d[iy, ix] *= Function2D.CosEdgeEllp(x: kx, y: ky, a: FilterKx, b: FilterKy, e: 0.0);
                        });
                    });
                    multi.Stop();
                    multiplication += multi.Elapsed;

                    // Return the output
                    convert.Start();
                    MatrixZ output = new MatrixZ(input.Rows, input.Cols);
                    output[new LongRange(0, n), new LongRange(0, input.Cols)] = a;
                    output[new LongRange(n, 2 * n), new LongRange(0, input.Cols)] = b;
                    output[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)] = c;
                    output[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)] = d;
                    convert.Stop();
                    converting += convert.Elapsed;
                    cal.Stop();
                    calculating += cal.Elapsed;

                    return output;
                }
            }

            #endregion


        }

        // general cases...


        // y-invariant cases



        /// <summary>
        /// for 1D y-invariant isotropic cases and ky=0
        /// </summary>
        public class MaxwellODE1D()
        {
            #region properties

            /// <summary>
            /// the medium of the layer
            /// </summary>
            public Layer1DMedium Medium { get; set; }

            /// <summary>
            /// gridinfo in k-domain
            /// </summary>
            public GridInfo1D GridK { get; set; }

            /// <summary>
            /// half width of the k-domain filter to get rid of evenescent wave
            /// </summary>
            public double FilterWidth { get; set; }

            /// <summary>
            /// gridinfo in x-domin
            /// </summary>
            public GridInfo1D GridX { get; set; }

            /// <summary>
            /// wavelength of the incident emfield
            /// </summary>
            public double Wavelength { get; set; }

            /// <summary>
            /// polarization of the incident emfield
            /// </summary>
            InPlanePolMode Polarization { get; set; }


            #endregion
            #region constructors

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="medium"> medium function </param>
            /// <param name="gridx"> gridinfo in x-domin </param>
            /// <param name="gridk"> gridinfo in k-domain </param>
            /// <param name="wavelength"> wavelength of the incident emfield </param>
            /// <param name="filterWidth"> half width of the k-domain filter to get rid of evenescent wave </param>
            /// <param name="polarization"> polarization of the incident emfield </param>
            public MaxwellODE1D(Layer1DMedium medium, 
                GridInfo1D gridx, 
                GridInfo1D gridk, 
                double wavelength, 
                double filterWidth, 
                InPlanePolMode polarization = InPlanePolMode.TE) : this()
            {
                Medium = medium;
                GridK = gridk;
                GridX = gridx;
                FilterWidth = filterWidth;
                Wavelength = wavelength;
                Polarization = polarization;
            }

            #endregion
            #region methods

            /// <summary>
            /// construct the maxwell ode in k-domain
            /// </summary>
            /// <param name="z"> the coordinate of propagate direction </param>
            /// <param name="input"> the vector contain E in front and H behind </param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            internal VectorZ dFz(VectorZ input)
            {
                //caluculate k0
                double k0 = 2 * Math.PI / Wavelength;

                //turn input vector into E and H
                if (input.Count % 2 != 0) { throw new ArgumentException(); }
                long n = input.Count / 2;
                VectorZ E = new VectorZ(input[new LongRange(0, n)]);
                VectorZ H = new VectorZ(input[new LongRange(n, 2 * n)]);

                //get epsilon and sampled
                VectorZ epsilon = Medium.Sample(Wavelength, GridX, MaterialProperty.Epsilon);

                //parameters
                VectorZ a = new VectorZ(E);
                VectorZ b = new VectorZ(H);
                GridInfo1D gridK = GridK;

                //for different polarization cases
                switch (Polarization)
                {
                    case InPlanePolMode.TE:
                        // for epsilon inhomogeneous
                        if (Medium.Mu == null)
                        {
                            //convolution by FFT
                            VectorZ convE = new VectorZ(E);
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Backward);
                            convE *= epsilon;
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Forward);

                            //maxwell ode (ky = 0)
                            Action<long> TE = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (-H[i]) * k0 * Complex.ImaginaryOne;
                                b[i] = (-convE[i] + kx * kx * E[i] / k0 / k0) * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TE, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);

                            //return value
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;

                            return output;
                        }
                        // for epsilon and mu inhomogeneous cases
                        else
                        {
                            // get mu
                            VectorZ mu = Medium.Sample(Wavelength, GridX, MaterialProperty.Mu);

                            //convolution by FFT
                            VectorZ convEe = new VectorZ(E);
                            VectorZ convEm = new VectorZ(E);
                            VectorZ convH = new VectorZ(H);
                            //firstly caluculate nx * E in k-domain
                            Action<long> conv = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                convEm[i] *= kx / k0;
                            };
                            Loop1D loop0 = new Loop1D(conv, 0, gridK.Count);
                            loop0.Evaluate(LoopMode.Parallel);
                            //then do the concolution
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convEe, ref gridK, FTOption.Backward);
                            convEe *= epsilon;
                            Transform.FFT1D(ref convEe, ref gridK, FTOption.Forward);
                            //conv(1 / mu, nx * E)
                            Transform.FFT1D(ref convEm, ref gridK, FTOption.Backward);
                            convEm /= mu;
                            Transform.FFT1D(ref convEm, ref gridK, FTOption.Forward);
                            //conv(mu, H)
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Backward);
                            convH *= mu;
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Forward);

                            //maxwell ode
                            Action<long> TE = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (-convH[i]) * k0 * Complex.ImaginaryOne;
                                b[i] = (-convEe[i] + kx * convEm[i] / k0) * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TE, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);

                            //return value
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            return output;
                        }
                    case InPlanePolMode.TM:
                        // for epsilon inhomogeneous
                        if (Medium.Mu == null)
                        {
                            //convolution
                            VectorZ convE = new VectorZ(E);
                            VectorZ convH = new VectorZ(H);
                            //firstly caluculate nx * H in k-domain
                            Action<long> conv = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                convH[i] *= kx / k0;
                            };
                            Loop1D loop0 = new Loop1D(conv, 0, gridK.Count);
                            loop0.Evaluate(LoopMode.Parallel);
                            //then do the concolution
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Backward);
                            convE *= epsilon;
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Forward);
                            //conv(1 / epsilon, H)
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Backward);
                            convH /= epsilon;
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Forward);

                            //maxwell ode
                            Action<long> TM = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (H[i] - kx * convH[i] / k0) * k0 * Complex.ImaginaryOne;
                                b[i] = convE[i] * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TM, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);

                            //return value
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            return output;
                        }
                        // for epsilon and mu inhomogeneous cases
                        else
                        {
                            // get mu
                            VectorZ mu = Medium.Sample(Wavelength, GridX, MaterialProperty.Mu);

                            //convolution
                            VectorZ convE = new VectorZ(E);
                            VectorZ convHe = new VectorZ(H);
                            VectorZ convHm = new VectorZ(H);
                            //firstly caluculate nx * H in k-domain
                            Action<long> conv = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                convHe[i] *= kx / k0;
                            };
                            Loop1D loop3 = new Loop1D(conv, 0, gridK.Count);
                            loop3.Evaluate(LoopMode.Parallel);
                            //then do the concolution
                            //conv(mu, H)
                            Transform.FFT1D(ref convHm, ref gridK, FTOption.Backward);
                            convHm *= mu;
                            Transform.FFT1D(ref convHm, ref gridK, FTOption.Forward);
                            //conv(1 / epsilon, nx * H)
                            Transform.FFT1D(ref convHe, ref gridK, FTOption.Backward);
                            convHe /= epsilon;
                            Transform.FFT1D(ref convHe, ref gridK, FTOption.Forward);
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Backward);
                            convE *= epsilon;
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Forward);

                            //maxwell ode
                            Action<long> TM = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (convHm[i] - kx * convHe[i] / k0) * k0 * Complex.ImaginaryOne;
                                b[i] = convE[i] * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TM, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);

                            //return value
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            return output;
                        }
                    default:
                        throw new ArgumentException();
                }

            }

            /// <summary>
            /// Maxwell ODE with timespan
            /// </summary>
            /// <param name="input"> input field </param>
            /// <param name="calculating"> timespan for the whole calculation </param>
            /// <param name="converting"> timespan for the converting between 2 field vectors and 1 output vector </param>
            /// <param name="convolution"> timespan for convolution </param>
            /// <param name="multiplication"> timespan for multiplication </param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            internal VectorZ dFz (VectorZ input,            
            ref TimeSpan calculating,
            ref TimeSpan converting,
            ref TimeSpan convolution,
            ref TimeSpan multiplication)
            {
                Stopwatch cal = Stopwatch.StartNew();
                cal.Start();
                //caluculate k0
                double k0 = 2 * Math.PI / Wavelength;

                Stopwatch convert = Stopwatch.StartNew();
                convert.Start();
                //turn input vector into E and H
                if (input.Count % 2 != 0) { throw new ArgumentException(); }
                long n = input.Count / 2;
                VectorZ E = new VectorZ(input[new LongRange(0, n)]);
                VectorZ H = new VectorZ(input[new LongRange(n, 2 * n)]);
                convert.Stop();

                //get epsilon and sampled
                VectorZ epsilon = Medium.Sample(Wavelength, GridX, MaterialProperty.Epsilon);

                //parameters
                VectorZ a = new VectorZ(E);
                VectorZ b = new VectorZ(H);
                GridInfo1D gridK = GridK;
                
                //for different polarization cases
                switch (Polarization)
                {
                    case InPlanePolMode.TE:
                        // for epsilon inhomogeneous
                        if (Medium.Mu == null)
                        {
                            //convolution by FFT
                            Stopwatch convo = Stopwatch.StartNew();
                            convo.Start();
                            VectorZ convE = new VectorZ(E);
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Backward);
                            convE *= epsilon;
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Forward);
                            convo.Stop();
                            convolution += convo.Elapsed;

                            //maxwell ode (ky = 0)
                            Stopwatch multi = Stopwatch.StartNew();
                            multi.Start();
                            Action<long> TE = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (-H[i]) * k0 * Complex.ImaginaryOne;
                                b[i] = (-convE[i] + kx * kx * E[i] / k0 / k0) * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TE, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);
                            multi.Stop();
                            multiplication += multi.Elapsed;

                            //return value
                            convert.Start();
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            convert.Stop();
                            converting += convert.Elapsed;
                            cal.Stop();
                            calculating += cal.Elapsed;

                            return output;
                        }
                        // for epsilon and mu inhomogeneous cases
                        else
                        {
                            // get mu
                            VectorZ mu = Medium.Sample( Wavelength, GridX, MaterialProperty.Mu);

                            //convolution by FFT
                            Stopwatch convo = Stopwatch.StartNew();
                            convo.Start();
                            VectorZ convEe = new VectorZ(E);
                            VectorZ convEm = new VectorZ(E);
                            VectorZ convH = new VectorZ(H);
                            //firstly caluculate nx * E in k-domain
                            Action<long> conv = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                convEm[i] *= kx / k0;
                            };
                            Loop1D loop0 = new Loop1D(conv, 0, gridK.Count);
                            loop0.Evaluate(LoopMode.Parallel);
                            //then do the concolution
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convEe, ref gridK, FTOption.Backward);
                            convEe *= epsilon;
                            Transform.FFT1D(ref convEe, ref gridK, FTOption.Forward);
                            //conv(1 / mu, nx * E)
                            Transform.FFT1D(ref convEm, ref gridK, FTOption.Backward);
                            convEm /= mu;
                            Transform.FFT1D(ref convEm, ref gridK, FTOption.Forward);
                            //conv(mu, H)
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Backward);
                            convH *= mu;
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Forward);
                            convo.Stop();
                            convolution += convo.Elapsed;

                            //maxwell ode
                            Stopwatch multi = Stopwatch.StartNew();
                            multi.Start();
                            Action<long> TE = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (-convH[i]) * k0 * Complex.ImaginaryOne;
                                b[i] = (-convEe[i] + kx * convEm[i] / k0) * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TE, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);
                            multi.Stop();
                            multiplication += multi.Elapsed;

                            //return value
                            convert.Start();
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            convert.Stop();
                            converting += convert.Elapsed;
                            cal.Stop();
                            calculating += cal.Elapsed;
                            return output;
                        }
                    case InPlanePolMode.TM:
                        // for epsilon inhomogeneous
                        if (Medium.Mu == null)
                        {
                            //convolution
                            Stopwatch convo = Stopwatch.StartNew();
                            convo.Start();
                            VectorZ convE = new VectorZ(E);
                            VectorZ convH = new VectorZ(H);
                            //firstly caluculate nx * H in k-domain
                            Action<long> conv = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                convH[i] *= kx / k0;
                            };
                            Loop1D loop0 = new Loop1D(conv, 0, gridK.Count);
                            loop0.Evaluate(LoopMode.Parallel);
                            //then do the concolution
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Backward);
                            convE *= epsilon;
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Forward);
                            //conv(1 / epsilon, H)
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Backward);
                            convH /= epsilon;
                            Transform.FFT1D(ref convH, ref gridK, FTOption.Forward);
                            convo.Stop();
                            convolution += convo.Elapsed;

                            //maxwell ode
                            Stopwatch multi = Stopwatch.StartNew();
                            multi.Start();
                            Action<long> TM = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (H[i] - kx * convH[i] / k0) * k0 * Complex.ImaginaryOne;
                                b[i] = convE[i] * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TM, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);
                            multi.Stop();
                            multiplication += multi.Elapsed;

                            //return value
                            convert.Start();
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            convert.Stop();
                            converting += convert.Elapsed;
                            cal.Stop();
                            calculating += cal.Elapsed;
                            return output;
                        }
                        // for epsilon and mu inhomogeneous cases
                        else
                        {
                            // get mu
                            VectorZ mu = Medium.Sample(Wavelength, GridX, MaterialProperty.Mu);

                            //convolution
                            Stopwatch convo = Stopwatch.StartNew();
                            convo.Start();
                            VectorZ convE = new VectorZ(E);
                            VectorZ convHe = new VectorZ(H);
                            VectorZ convHm = new VectorZ(H);
                            //firstly caluculate nx * H in k-domain
                            Action<long> conv = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                convHe[i] *= kx / k0;
                            };
                            Loop1D loop3 = new Loop1D(conv, 0, gridK.Count);
                            loop3.Evaluate(LoopMode.Parallel);
                            //then do the concolution
                            //conv(mu, H)
                            Transform.FFT1D(ref convHm, ref gridK, FTOption.Backward);
                            convHm *= mu;
                            Transform.FFT1D(ref convHm, ref gridK, FTOption.Forward);
                            //conv(1 / epsilon, nx * H)
                            Transform.FFT1D(ref convHe, ref gridK, FTOption.Backward);
                            convHe /= epsilon;
                            Transform.FFT1D(ref convHe, ref gridK, FTOption.Forward);
                            //conv(epsilon, E)
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Backward);
                            convE *= epsilon;
                            Transform.FFT1D(ref convE, ref gridK, FTOption.Forward);
                            convo.Stop();
                            convolution += convo.Elapsed;

                            //maxwell ode
                            Stopwatch multi = Stopwatch.StartNew();
                            multi.Start(); 
                            Action<long> TM = i =>
                            {
                                var kx = gridK.GetCoordinate(i);
                                a[i] = (convHm[i] - kx * convHe[i] / k0) * k0 * Complex.ImaginaryOne;
                                b[i] = convE[i] * k0 * Complex.ImaginaryOne;
                                a[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                                b[i] *= Function1D.CosEdgeRect(kx, 2 * FilterWidth, 0);
                            };
                            Loop1D loop = new Loop1D(TM, 0, gridK.Count);
                            loop.Evaluate(LoopMode.Parallel);
                            multi.Stop();
                            multiplication += multi.Elapsed;

                            //return value
                            convert.Start();
                            VectorZ output = new VectorZ(2 * n);
                            output[new LongRange(0, n)] = a;
                            output[new LongRange(n, 2 * n)] = b;
                            convert.Stop();
                            converting += convert.Elapsed;
                            cal.Stop();
                            calculating += cal.Elapsed;
                            return output;
                        }
                    default:
                        throw new ArgumentException();
                }
                
            }
            #endregion
        }

    }
      


  
}
