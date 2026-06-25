using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Rayleigh-Sommerfeld diffraction integral
    /// </summary>
    internal class RayleighSommerfeld
    {

        #region ---- 1D kernel [pointwise] ----

        /// <summary>
        /// Calculates the Rayleigh-Sommerfeld diffraction integral for a 1D input field.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum (meters).</param>
        /// <param name="nReal">Real part of the refractive index at the specified wavelength.</param>
        /// <param name="vIn">Input field values sampled along the x-domain.</param>
        /// <param name="gIn">Sampling grid information for the input field.</param>
        /// <param name="x">Target lateral position x (meters) where the field is evaluated.</param>
        /// <param name="z">Propagation distance z (meters) from the input plane to the target point.</param>
        /// <param name="loopMode">Loop-computational mode option for evaluating the integral (e.g., sequential or parallel).</param>
        /// <returns>
        /// The complex field value at the specified (x, z) position, calculated using the Rayleigh-Sommerfeld integral.
        /// </returns>
        internal static Complex Propagate1D(double wavelength, double nReal,
            VectorZ vIn, GridInfo1D gIn,
            double x, double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            double k0 = 2.0 * Math.PI / wavelength;
            double k = nReal * k0;

            // Precompute z^2 for efficiency
            double z2 = z * z;
            Complex vOut = Complex.Zero;

            // defines loop operation
            void op(long i)
            {
                double xIn = gIn[i];
                double dx = x - xIn;
                double rho = Math.Sqrt(dx * dx + z2);
                double cosTheta = z / rho;
                Complex t = k * SpecialFunctions.HankelH1(n: 1, z: k * rho) * cosTheta;
                vOut += vIn[i, false] * t;
            }
            Loop1D loop = new(operation: op, start: 0, end: gIn.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return 0.5 * Complex.ImaginaryOne * gIn.Spacing * vOut;
        }

        ///// <summary>
        ///// calculates the field value at (x, z) away
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field component in x-domain </param>
        ///// <param name="vInEy"> input Ey field component in x-domain </param>
        ///// <param name="gIn"> sampling grid of input field </param>
        ///// <param name="x"> lateral position x </param>
        ///// <param name="z"> propagation distance z </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> complex (Ex, Ey) field value at (x, z) </returns>
        //internal static (Complex, Complex) Propagate1D(double wavelength, double nReal,
        //    VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
        //    double x, double z,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    // initialization
        //    Complex vOutEx = Complex.Zero;
        //    Complex vOutEy = Complex.Zero;
        //    double k0 = 2.0 * Math.PI / wavelength;
        //    double k = nReal * k0;

        //    // defines loop operation
        //    Action<long> a = (i) =>
        //    {
        //        double xIn = gIn[i];
        //        double dx = x - xIn;
        //        double rho = Math.Sqrt(dx * dx + z * z);
        //        double cosTheta = z / rho;
        //        Complex t = k * SpecialFunctions.HankelH1(n: 1, z: k * rho) * cosTheta;
        //        vOutEx += vInEx[i, false] * t;
        //        vOutEy += vInEy[i, false] * t;
        //    };
        //    Loop1D loop = new(operation: a, start: 0, end: gIn.Count, step: 1);
        //    loop.Evaluate(mode: loopMode);

        //    // return
        //    Complex fac = 0.5 * Complex.ImaginaryOne * gIn.Spacing;
        //    return (fac * vOutEx, fac * vOutEy);
        //}

        #endregion
        #region ---- 1D kernel [point-loop] ----   

        /// <summary>
        /// Calculates the Rayleigh-Sommerfeld diffraction integral for a 1D input field at multiple target positions.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum (meters).</param>
        /// <param name="nReal">Real part of the refractive index at the specified wavelength.</param>
        /// <param name="vIn">Input field values sampled along the x-domain.</param>
        /// <param name="gIn">Sampling grid information for the input field.</param>
        /// <param name="xs">Set of target x-positions (meters) where the field is evaluated.</param>
        /// <param name="zs">Set of target z-positions (meters) corresponding to each x-position.</param>
        /// <param name="loopMode">Computational option for loop evaluation (e.g., sequential or parallel).</param>
        /// <returns>
        /// A <see cref="VectorZ"/> containing the complex field values at the specified (x, z) positions, calculated using the Rayleigh-Sommerfeld integral.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the number of x-positions does not match the number of z-positions.</exception>
        internal static VectorZ Propagate1D(double wavelength, double nReal,
            VectorZ vIn, GridInfo1D gIn,
            ScatInfo1D xs, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (zs.Count != xs.Count)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initiaization
            VectorZ vOut = new(count: xs.Count);

            // defines loop operation
            void op(long i)
            {
                vOut[i, false] = Propagate1D(wavelength: wavelength, nReal: nReal,
                    vIn: vIn, gIn: gIn,
                    x: xs[i, false], z: zs[i, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            }
            Loop1D loop = new(operation: op, start: 0, end: vOut.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// Calculates the Rayleigh-Sommerfeld diffraction integral for a 1D input field at a set of uniformly distributed target positions.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum (meters).</param>
        /// <param name="nReal">Real part of the refractive index at the specified wavelength.</param>
        /// <param name="vIn">Input field values sampled along the x-domain.</param>
        /// <param name="gIn">Sampling grid information for the input field.</param>
        /// <param name="gTarget">Target grid of output field positions (uniformly distributed in x).</param>
        /// <param name="zs">Set of target z-positions (meters) corresponding to each x-position in <paramref name="gTarget"/>.</param>
        /// <param name="loopMode">Computational option for loop evaluation (e.g., sequential or parallel).</param>
        /// <returns>
        /// A <see cref="VectorZ"/> containing the complex field values at the specified (x, z) positions, calculated using the Rayleigh-Sommerfeld integral.
        /// </returns>
        internal static VectorZ Propagate1D(double wavelength, double nReal,
            VectorZ vIn, GridInfo1D gIn,
            GridInfo1D gTarget, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
            => Propagate1D(wavelength: wavelength, nReal: nReal, vIn: vIn, gIn: gIn,
                xs: (ScatInfo1D)gTarget, zs: zs, loopMode: loopMode);

        ///// <summary>
        ///// calculates the field values at a series of 
        ///// arbitary (x, z)-positions 
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field component in x-domain </param>
        ///// <param name="vInEy"> input Ey field component in x-domain </param>
        ///// <param name="gIn"> sampling grid of both input field components </param>
        ///// <param name="xs"> set of target x-positions </param>
        ///// <param name="zs"> set of target z-positions </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> complex (Ex, Ey) field value at (x, z)-positions </returns>
        //internal static (VectorZ, VectorZ) Propagate1D(double wavelength, double nReal,
        //    VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
        //    ScatInfo1D xs, VectorD zs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    if (zs.Count != xs.Count)
        //    { throw new ArgumentException($"Unequal number of input elements"); }

        //    // initiaization
        //    VectorZ vOutEx = new(count: xs.Count);
        //    VectorZ vOutEy = new(count: xs.Count);

        //    // defines loop operation
        //    Action<long> a = (i) =>
        //    {
        //        (vOutEx[i, false], vOutEy[i, false]) = Propagate1D(wavelength: wavelength, nReal: nReal,
        //            vInEx: vInEx, vInEy: vInEy, gIn: gIn,
        //            z: zs[i, false], x: xs[i, false],
        //            loopMode: LoopMode.Sequential); // sets inner loop to sequential
        //    };
        //    Loop1D loop = new(operation: a,
        //        start: 0, end: vOutEx.Count, step: 1);
        //    loop.Evaluate(mode: loopMode);

        //    // return
        //    return (vOutEx, vOutEy);
        //}

        ///// <summary>
        ///// calculates the field values at a series of 
        ///// (x, z)-positions with uniform x distributions
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field component in x-domain </param>
        ///// <param name="vInEy"> input Ey field component in x-domain </param>
        ///// <param name="gIn"> sampling grid of both input field components </param>
        ///// <param name="gTarget"> target grid of oupput field components </param>
        ///// <param name="zs"> set of target z-positions </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> complex (Ex, Ey) field value at (x, z)-positions </returns>
        //internal static (VectorZ, VectorZ) Propagate1D(double wavelength, double nReal,
        //    VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
        //    GridInfo1D gTarget, VectorD zs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Propagate1D(wavelength: wavelength, nReal: nReal, vInEx: vInEx, vInEy: vInEy, gIn: gIn,
        //        xs: (ScatInfo1D)gTarget, zs: zs, loopMode: loopMode);

        #endregion


        #region ---- 2D kernel [pointwise] ----

        /// <summary>
        /// Calculates the Rayleigh-Sommerfeld diffraction integral for a 2D input field at a specified target position.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum (meters).</param>
        /// <param name="nReal">Real part of the refractive index at the specified wavelength.</param>
        /// <param name="vIn">Values of the input field sampled on the 2D grid.</param>
        /// <param name="gIn">Sampling grid information for the input field.</param>
        /// <param name="x">Target lateral position x (meters) where the field is evaluated.</param>
        /// <param name="y">Target lateral position y (meters) where the field is evaluated.</param>
        /// <param name="z">Propagation distance z (meters) from the input plane to the target point.</param>
        /// <param name="loopMode">Loop-computational mode option for evaluating the integral (e.g., sequential or parallel).</param>
        /// <returns>
        /// The complex field value at the specified (x, y, z) position, calculated using the Rayleigh-Sommerfeld integral.
        /// </returns>
        internal static Complex Propagate2D(double wavelength, double nReal,
            MatrixZ vIn, GridInfo2D gIn,
            double x, double y, double z, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            double k0 = 2.0 * Math.PI / wavelength;
            double k = nReal * k0;
            Complex imagOne = Complex.ImaginaryOne;

            // Precompute z^2 for efficiency
            double z2 = z * z;
            Complex vOut = Complex.Zero;

            // defines loop operation
            Action<long, long> op = (iRow, iCol) =>
            {
                // coordinates
                (double yi, double xi) = gIn[iRow, iCol];
                double dy = y - yi;
                double dx = x - xi;
                // further parameters
                double r2 = dx * dx + dy * dy + z2;
                double r = Math.Sqrt(r2);
                double r3 = r2 * r;
                double kR = k * r;
                // terms in the integral
                Complex t = z / r3 * (1.0 - imagOne * kR) * Complex.Exp(imagOne * kR);
                // adds to result
                vOut += vIn[iRow, iCol, false] * t;
            };
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: gIn.Rows,
                colStart: 0, colEnd: gIn.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut * gIn.SpacingX * gIn.SpacingY / (2.0 * Math.PI);
        }

        ///// <summary>
        ///// calculates the field value at (x, y, z) away
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field component in x-domain </param>
        ///// <param name="vInEy"> input Ey field component in x-domain </param>
        ///// <param name="gIn"> sampling grid of both input field components </param>
        ///// <param name="x"> target lateral position x </param>
        ///// <param name="y"> target lateral position y </param>
        ///// <param name="z"> target longitudinal distance z </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        ///// <returns> complex (Ex, Ey) field value at (x, y, z) </returns>
        //internal static (Complex, Complex) Propagate2D(double wavelength, double nReal,
        //    MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
        //    double x, double y, double z,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    // initialization
        //    Complex vOutEx = Complex.Zero;
        //    Complex vOutEy = Complex.Zero;
        //    double k0 = 2.0 * Math.PI / wavelength;
        //    double k = k0 * nReal;

        //    // defines loop operation
        //    Action<long, long> a = (iRow, iCol) =>
        //    {
        //        // coordinates
        //        (double yi, double xi) = gIn[iRow, iCol];
        //        double dy = y - yi;
        //        double dx = x - xi;
        //        // further parameters
        //        double r2 = dx * dx + dy * dy + z * z;
        //        double r = Math.Sqrt(r2);
        //        double r3 = r2 * r;
        //        double kR = k * r;
        //        // terms in the integral
        //        Complex t = z / r3 * (1.0 - Complex.ImaginaryOne * kR)
        //            * Complex.Exp(Complex.ImaginaryOne * kR);
        //        // adds to result
        //        vOutEx += vInEx[iRow, iCol, false] * t;
        //        vOutEx += vInEy[iRow, iCol, false] * t;
        //    };
        //    Loop2D loop = new(operation: a,
        //        rowStart: 0, rowEnd: gIn.Rows,
        //        colStart: 0, colEnd: gIn.Cols,
        //        rowStep: 1, colStep: 1);
        //    loop.Evaluate(mode: loopMode);

        //    // return
        //    double fac = gIn.SpacingX * gIn.SpacingY / (2.0 * Math.PI);
        //    return (fac * vOutEx, fac * vOutEy); 
        //}

        #endregion
        #region ---- 2D kernel [point-loop] ----

        /// <summary>
        /// calculates the field values a series of 
        /// arbitary (x,y, z)-positions 
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> real-part of refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in x-domain </param>
        /// <param name="gIn"> sampling grid of input field </param>
        /// <param name="xys"> set of target (x, y)-positions </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at (x,y, z)-positions </returns>
        internal static VectorZ Propagate2D(double wavelength, double nReal,
            MatrixZ vIn, GridInfo2D gIn,
            ScatInfo2D xys, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if(xys.Count != zs.Count)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initiaization
            VectorZ vOut = new(count: xys.Count);

            // defines loop operation
            Action<long> op = (i) =>
            {
                (double yi, double xi) = xys[i, false];
                vOut[i, false] = Propagate2D(wavelength: wavelength, nReal: nReal,
                    vIn: vIn, gIn: gIn,
                    x: xi, y: yi, z: zs[i, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop1D loop = new(operation: op, start: 0, end: xys.Count, step: 1);
            loop.Evaluate(mode: loopMode);
                
            // return
            return vOut;
        }

        /// <summary>
        /// calculates the field values a series of 
        /// (x,y, z)-positions with separable x,y distributions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> real-part of refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in x-domain </param>
        /// <param name="gIn"> sampling grid of input field </param>
        /// <param name="xys"> set of target (x, y)-positions </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at (x,y, z)-positions </returns>
        internal static MatrixZ Propagate2D(double wavelength, double nReal,
            MatrixZ vIn, GridInfo2D gIn,
            ScatInfoXY xys, MatrixD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if(xys.Rows != zs.Rows || xys.Cols != zs.Cols)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initialization
            MatrixZ vOut = new(rows: xys.Rows, cols: xys.Cols);

            // defines loop operation
            Action<long, long> op = (iRow, iCol) =>
            {
                (double yi, double xi) = xys[iRow, iCol, false];
                vOut[iRow, iCol, false] = Propagate2D(wavelength: wavelength, nReal: nReal,
                    vIn: vIn, gIn: gIn,
                    x: xi, y: yi, z: zs[iRow, iCol, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: xys.Rows,
                colStart: 0, colEnd: xys.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// calculates the field values a series of 
        /// (x,y, z)-positions with uniform x,y distributions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> real-part of refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in x-domain </param>
        /// <param name="gIn"> sampling grid of input field </param>
        /// <param name="gTarget"> target grid of output field </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at (x,y, z)-positions </returns>
        internal static MatrixZ Propagate2D(double wavelength, double nReal,
            MatrixZ vIn, GridInfo2D gIn,
            GridInfo2D gTarget, MatrixD zs,
            LoopMode loopMode = Defaults.LoopOption)
            => Propagate2D(wavelength: wavelength, nReal: nReal, vIn: vIn, gIn: gIn,
                xys: (ScatInfoXY)gTarget, zs: zs, loopMode: loopMode);

        ///// <summary>
        ///// calculates the field values a series of 
        ///// arbitary (x,y, z)-positions 
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field in k-domain </param>
        ///// <param name="vInEy"> input Ey field in k-domain </param>
        ///// <param name="gIn"> sampling grid of both input field components </param>
        ///// <param name="xys"> set of target (x, y)-positions </param>
        ///// <param name="zs"> z-distances at all the (x, y)-positions </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> complex (Ex, Ey) field values at (x,y, z)-positions </returns>
        //internal static (VectorZ, VectorZ) Propagate2D(double wavelength, double nReal,
        //    MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
        //    ScatInfo2D xys, VectorD zs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    // consistency check
        //    if (xys.Count != zs.Count)
        //    { throw new ArgumentException($"Unequal number of input elements"); }

        //    // initiaization
        //    VectorZ vOutEx = new(count: xys.Count);
        //    VectorZ vOutEy = new(count: xys.Count);

        //    // defines loop operation
        //    Action<long> a = (i) =>
        //    {
        //        (double xi, double yi) = xys[i, false];
        //        (Complex vEx, Complex vEy) = Propagate2D(wavelength: wavelength, nReal: nReal,
        //            vInEx: vInEx, vInEy: vInEy, gIn: gIn,
        //            x: xi, y: yi, z: zs[i, false],
        //            loopMode: LoopMode.Sequential); // sets inner loop to sequential
        //        vOutEx[i, false] = vEx;
        //        vOutEy[i, false] = vEy;
        //    };
        //    Loop1D loop = new(operation: a, start: 0, end: xys.Count, step: 1);
        //    loop.Evaluate(mode: loopMode);

        //    // return 
        //    return (vOutEx, vOutEy);
        //}

        ///// <summary>
        ///// calculates the field values a series of 
        ///// (x,y, z)-positions with separable x,y distributions
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field in k-domain </param>
        ///// <param name="vInEy"> input Ey field in k-domain </param>
        ///// <param name="gIn"> sampling grid of both input field components </param>
        ///// <param name="xys"> set of target (x, y)-positions </param>
        ///// <param name="zs"> z-distances at all the (x, y)-positions </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> complex (Ex, Ey) field values at (x,y, z)-positions </returns>
        //internal static (MatrixZ, MatrixZ) Propagate2D(double wavelength, double nReal,
        //    MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
        //    ScatInfoXY xys, MatrixD zs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    // consistency check
        //    if (xys.Rows != zs.Rows || xys.Cols != zs.Cols)
        //    { throw new ArgumentException($"Unequal number of input elements"); }

        //    // initialization
        //    MatrixZ vOutEx = new(rows: xys.Rows, cols: xys.Cols);
        //    MatrixZ vOutEy = new(rows: xys.Rows, cols: xys.Cols);

        //    // defines loop operation
        //    Action<long, long> a = (iRow, iCol) =>
        //    {
        //        (double yi, double xi) = xys[iRow, iCol, false];
        //        (Complex vEx, Complex vEy) = Propagate2D(wavelength: wavelength, nReal: nReal,
        //            vInEx: vInEx, vInEy: vInEy, gIn: gIn,
        //            x: xi, y: yi, z: zs[iRow, iCol, false],
        //            loopMode: LoopMode.Sequential); // sets inner loop to sequential
        //        vOutEx[iRow, iCol, false] = vEx;
        //        vOutEy[iRow, iCol, false] = vEy;
        //    };
        //    Loop2D loop = new(operation: a,
        //        rowStart: 0, rowEnd: xys.Rows,
        //        colStart: 0, colEnd: xys.Cols,
        //        rowStep: 1, colStep: 1);
        //    loop.Evaluate(loopMode);

        //    // return
        //    return (vOutEx, vOutEy);
        //}

        ///// <summary>
        ///// calculates the field values a series of 
        ///// (x,y, z)-positions with uniform x,y distributions
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of refractive index @wavelength </param>
        ///// <param name="vInEx"> input Ex field in k-domain </param>
        ///// <param name="vInEy"> input Ey field in k-domain </param>
        ///// <param name="gIn"> sampling grid of both input field components </param>
        ///// <param name="gTarget"> target grid of output field components </param>
        ///// <param name="zs"> z-distances at all the (x, y)-positions </param>
        ///// <param name="loopMode"> computational option for loop </param>
        ///// <returns> complex (Ex, Ey) field values at (x,y, z)-positions </returns>
        //internal static (MatrixZ, MatrixZ) Propagate2D(double wavelength, double nReal,
        //    MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
        //    GridInfo2D gTarget, MatrixD zs,
        //    LoopMode loopMode = Defaults.LoopOption)
        //    => Propagate2D(wavelength: wavelength, nReal: nReal, vInEx: vInEx, vInEy: vInEy, gIn: gIn,
        //        xys: (ScatInfoXY)gTarget, zs: zs, loopMode: loopMode);

        #endregion

    }
}
