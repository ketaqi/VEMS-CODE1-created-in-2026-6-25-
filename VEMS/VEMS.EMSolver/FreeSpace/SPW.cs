using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// spectrum of plane waves
    /// </summary>
    public class SPW
    {

        #region ------- 1D raw [k-domain] -------

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> input field values in k-domain </param>
        /// <param name="gIn"> grid of input field </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> propagated field values in k-domain </returns>
        public static VectorZ Propagate1D(double wavelength, Complex n,
            VectorZ vIn, GridInfo1D gIn,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorZ vOut = new(count: vIn.Count);
            double k0 = 2.0 * Math.PI / wavelength;
            Complex k2 = k0 * n * k0 * n;

            // defines loop operation
            Action<long> a = (i) =>
            {
                double kx = gIn[i];
                Complex kz = Complex.Sqrt(k2 - kx * kx);
                vOut[i, false] = vIn[i, false] *
                    Complex.Exp(Complex.ImaginaryOne * kz * z);
            };
            Loop1D loop = new(operation: a, start: 0, end: gIn.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field component </param>
        /// <param name="vInEy"> input Ey field component </param>
        /// <param name="gIn"> grid of both input field components </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> propagated (Ex, Ey) field components in k-domain </returns>
        internal static (VectorZ, VectorZ) Propagate1D(double wavelength, Complex n,
            VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            VectorZ vOutEx = new(count: vInEx.Count);
            VectorZ vOutEy = new(count: vInEy.Count);
            double k0 = 2.0 * Math.PI / wavelength;
            Complex k2 = k0 * n * k0 * n;

            // defines loop operation
            Action<long> a = (i) =>
            {
                double kx = gIn[i];
                Complex kz = Complex.Sqrt(k2 - kx * kx);
                Complex p = Complex.Exp(Complex.ImaginaryOne * kz * z);
                vOutEx[i, false] = vInEx[i, false] * p;
                vOutEy[i, false] = vInEy[i, false] * p;
            };
            Loop1D loop = new(operation: a, start: 0, end: gIn.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (vOutEx, vOutEy);
        }

        #endregion
        #region ------- 1D raw [pointwise] --------

        /// <summary>
        /// calculates the field value at (x, z) away from input
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in k-domain </param>
        /// <param name="gIn"> grid of input field </param>
        /// <param name="x"> target lateral position x </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> complex field value at (x, z) in x-domain </returns>
        internal static Complex Propagate1D(double wavelength, Complex n,
            VectorZ vIn, GridInfo1D gIn,
            double x, double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // propagates in k-domain first
            VectorZ vOut = Propagate1D(wavelength: wavelength, n: n,
                vIn: vIn, gIn: gIn,
                z: z, loopMode: loopMode);
            // inverse DFT
            return Transform.DFT(x: vOut, grid: gIn, t: x, opt: DFTOption.Backward,
                loopMode: loopMode);
        }

        /// <summary>
        /// calculates the field value at (x, z) away from input
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> grid of both input field components </param>
        /// <param name="x"> target lateral position x </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> complex (Ex, Ey) field value at (x, z) in x-domain </returns>
        internal static (Complex, Complex) Propagate1D(double wavelength, Complex n,
            VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
            double x, double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // propagates in k-domain first
            (VectorZ vEx, VectorZ vEy) = Propagate1D(wavelength: wavelength, n: n,
                vInEx: vInEx, vInEy: vInEy, gIn: gIn,
                z: z, loopMode: loopMode);
            // inverse DFT
            Complex vOutEx = Transform.DFT(x: vEx, grid: gIn, t: x, opt: DFTOption.Backward,
                loopMode: loopMode);
            Complex vOutEy = Transform.DFT(x: vEy, grid: gIn, t: x, opt: DFTOption.Backward,
                loopMode: loopMode);
            return (vOutEx, vOutEy);
        }

        #endregion
        #region ------- 1D raw [point-loop] -------

        /// <summary>
        /// calculates the field values at a series of
        /// arbitrary (x, z)-positions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in k-domain </param>
        /// <param name="gIn"> grid of input field </param>
        /// <param name="xs"> set of target x-positions </param>
        /// <param name="zs"> z-distances for all the x-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at all (x, z)-positions </returns>
        internal static VectorZ Propagate1D(double wavelength, Complex n,
            VectorZ vIn, GridInfo1D gIn,
            ScatInfo1D xs, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (zs.Count != xs.Count)
            { throw new ArgumentException($"Unequal input number of vector elements"); }

            // initialization
            VectorZ vOut = new(count: xs.Count);

            // defines loop operation
            Action<long> a = (i) =>
            {
                vOut[i, false] = Propagate1D(wavelength: wavelength, n: n,
                    vIn: vIn, gIn: gIn,
                    x: xs[i, false], z: zs[i, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop1D loop = new(operation: a, start: 0, end: vOut.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// calculates the field values at a series of
        /// (x, z)-positions with uniform distributed x-locations
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in k-domain </param>
        /// <param name="gIn"> grid of input field </param>
        /// <param name="gTarget"> target grid of output field </param>
        /// <param name="zs"> z-distances for all the x-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at all (x, z)-positions </returns>
        internal static VectorZ Propagate1D(double wavelength, Complex n,
            VectorZ vIn, GridInfo1D gIn,
            GridInfo1D gTarget, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
            => Propagate1D(wavelength, n, vIn, gIn, 
                (ScatInfo1D)gTarget, zs, loopMode);

        /// <summary>
        /// calculates the field values at a series of
        /// arbitrary (x, z)-positions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> grid of both input field components </param>
        /// <param name="xs"> set of target x-positions </param>
        /// <param name="zs"> z-distances for all the x-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> (Ex, Ey) field values at all (x, z)positions </returns>
        internal static (VectorZ, VectorZ) Propagate1D(double wavelength, Complex n,
            VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
            ScatInfo1D xs, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (zs.Count != xs.Count)
            { throw new ArgumentException($"Unequal input number of vector elements"); }

            // initialization
            VectorZ vOutEx = new(count: xs.Count);
            VectorZ vOutEy = new(count: xs.Count);

            // defines loop operation
            Action<long> a = (i) =>
            {
                (Complex vEx, Complex vEy) = Propagate1D(wavelength: wavelength, n: n,
                    vInEx: vInEx, vInEy: vInEy, gIn: gIn,
                    x: xs[i, false], z: zs[i, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential  
                vOutEx[i, false] = vEx;
                vOutEy[i, false] = vEy;
            };
            Loop1D loop = new(operation: a, start: 0, end: vOutEx.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (vOutEx, vOutEy);
        }

        /// <summary>
        /// calculates the field values at a series of
        /// (x, z)-positions with uniformly distributed x-locations
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> grid of both input field components </param>
        /// <param name="gTarget"> target grid of output field components </param>
        /// <param name="zs"> z-distances for all the x-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> (Ex, Ey) field values at all (x, z)positions </returns>
        internal static (VectorZ, VectorZ) Propagate1D(double wavelength, Complex n,
            VectorZ vInEx, VectorZ vInEy, GridInfo1D gIn,
            GridInfo1D gTarget, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
            => Propagate1D(wavelength, n, vInEx, vInEy, gIn, 
                (ScatInfo1D)gTarget, zs, loopMode);

        #endregion
        #region ------- 1D kernel [k-domain] -------

        /// <summary>
        /// Propagates a 1D field in the k-domain to a parallel plane at a given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method, with support for an additional linear phase term.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="v">Field values in k-domain (input/output).</param>
        /// <param name="nx">Normalized kx values (nx = kx / k0).</param>
        /// <param name="nz">Normalized kz values (nz = kz / k0).</param>
        /// <param name="z">Propagation distance along the z direction.</param>
        /// <param name="cLinear">Coefficient of the additional linear phase added to the propagation kernel in k-domain.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        internal static void Propagate1D(double wavelength,
            ref VectorZ v, VectorD nx, VectorZ nz,
            double z,
            double cLinear = 0.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if(v.Count != nz.Count) 
            { throw new ArgumentException($"Unequal number of input elements"); }

            Complex imOne = Complex.ImaginaryOne;
            double k0 = 2.0 * Math.PI / wavelength;
            VectorZ values = v;

            if (cLinear == 0.0)
            {
                void op(long i)
                {
                    Complex nzi = nz[i, false];
                    values[i, false] *= Complex.Exp(imOne * k0 * nzi * z);
                }
                new Loop1D(op, 0, v.Count, 1).Evaluate(loopMode);
            }
            else
            {
                void op(long i)
                {
                    double nxi = nx[i, false];
                    Complex nzi = nz[i, false];
                    values[i, false] *= Complex.Exp(imOne * k0 * (nzi * z + nxi * cLinear));
                }
                new Loop1D(op, 0, v.Count, 1).Evaluate(loopMode);
            }
        }


        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// with given nz = kz/k0 information
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="v"> field values in k-domain (in/out) </param>
        /// <param name="nz"> normalized kz => nz = kz/k0 </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        internal static void Propagate1D(double wavelength,
            ref VectorZ v, VectorZ nz,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (v.Count != nz.Count)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initialization
            double k0 = 2.0 * Math.PI / wavelength;

            // defines loop operation
            VectorZ values = v;
            void op(long i)
            {
                Complex kz = k0 * nz[i, false];
                values[i, false] *= Complex.Exp(Complex.ImaginaryOne * kz * z);
            }

            Loop1D loop = new(operation: op, start: 0, end: v.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// with given nz = kz/k0 information
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>        
        /// <param name="vEx"> Ex field in k-domain (in/out) </param>
        /// <param name="vEy"> Ey field in k-domain (in/out) </param>
        /// <param name="nz"> normalized kz => nz = kz/k0 </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        internal static void Propagate1D(double wavelength,
            ref VectorZ vEx, ref VectorZ vEy, VectorZ nz,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (vEx.Count != nz.Count || vEy.Count != nz.Count)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initialization
            double k0 = 2.0 * Math.PI / wavelength;

            // defines loop operation
            VectorZ valuesEx = vEx;
            VectorZ valuesEy = vEy;
            Action<long> a = (i) =>
            {
                Complex p = Complex.Exp(Complex.ImaginaryOne * k0 * nz[i, false] * z);
                valuesEx[i, false] *= p;
                valuesEy[i, false] *= p;
            };
            Loop1D loop = new(operation: a, start: 0, end: nz.Count, step: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion
        

        #region ------- 2D raw [k-domain] -------

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> input field values in k-domain </param>
        /// <param name="gIn"> grid of input field </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> propagated field values in k-domain </returns>
        internal static MatrixZ Propagate2D(double wavelength, Complex n,
            MatrixZ vIn, GridInfo2D gIn,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixZ vOut = new(rows: vIn.Rows, cols: vIn.Cols);
            double k0 = 2.0 * Math.PI / wavelength;
            Complex k2 = k0 * n * k0 * n;

            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double ky, double kx) = gIn[iRow, iCol];
                Complex kz = Complex.Sqrt(k2 - kx * kx - ky * ky);
                vOut[iRow, iCol, false] = vIn[iRow, iCol, false] *
                    Complex.Exp(Complex.ImaginaryOne * kz * z);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: gIn.Rows,
                colStart: 0, colEnd: gIn.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field component in k-domain </param>
        /// <param name="vInEy"> input Ey field component in k-domain </param>
        /// <param name="gIn"> grid of both input field components </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> propagated (Ex, Ey) field components in k-domain </returns>
        internal static (MatrixZ, MatrixZ) Propagate2D(double wavelength, Complex n,
            MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // initialization
            MatrixZ vOutEx = new(rows: vInEx.Rows, cols: vInEx.Cols);
            MatrixZ vOutEy = new(rows: vInEy.Rows, cols: vInEy.Cols);
            double k0 = 2.0 * Math.PI / wavelength;
            Complex k2 = k0 * n * k0 * n;

            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double ky, double kx) = gIn[iRow, iCol];
                Complex kz = Complex.Sqrt(k2 - kx * kx - ky * ky);
                Complex p = Complex.Exp(Complex.ImaginaryOne * kz * z);
                vOutEx[iRow, iCol, false] = vInEx[iRow, iCol, false] * p;
                vOutEy[iRow, iCol, false] = vInEy[iRow, iCol, false] * p;
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: gIn.Rows,
                colStart: 0, colEnd: gIn.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (vOutEx, vOutEy);
        }

        #endregion
        #region ------- 2D raw [pointwise] --------

        /// <summary>
        /// calculates the field value at (x, y, z) away from input
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in k-domain </param>
        /// <param name="gIn"> grid of input field </param>
        /// <param name="x"> target lateral position x </param>
        /// <param name="y"> target lateral position y </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> complex field value at (x, y, z) in x-domain </returns>
        internal static Complex Propagate2D(double wavelength, Complex n,
            MatrixZ vIn, GridInfo2D gIn,
            double x, double y, double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // propagates in k-domain first
            MatrixZ vOut = Propagate2D(wavelength: wavelength, n: n,
                vIn: vIn, gIn: gIn,
                z: z, loopMode: loopMode);
            // inverse DFT
            return Transform.DFT(x: vOut, grid: gIn,
                ty: y, tx: x, opt: DFTOption.Backward,
                loopMode: loopMode);
        }

        /// <summary>
        /// calculates the field value at (x, y, z) away from input
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> grid of both input field components </param>
        /// <param name="x"> target lateral position x </param>
        /// <param name="y"> target lateral position y </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> complex (Ex, Ey) field value at (x, y, z) in x-domain </returns>
        internal static (Complex, Complex) Propagate2D(double wavelength, Complex n,
            MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
            double x, double y, double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // propagates in k-domain first
            (MatrixZ vEx, MatrixZ vEy) = Propagate2D(wavelength: wavelength, n: n,
                vInEx: vInEx, vInEy: vInEy, gIn: gIn,
                z: z, loopMode: loopMode);
            // inverse DFT
            Complex vOutEx = Transform.DFT(x: vEx, grid: gIn,
                ty: y, tx: x, opt: DFTOption.Backward,
                loopMode: loopMode);
            Complex vOutEy = Transform.DFT(x: vEy, grid: gIn,
                ty: y, tx: x, opt: DFTOption.Backward,
                loopMode: loopMode);
            return (vOutEx, vOutEy);
        }

        #endregion
        #region ------- 2D raw [point-loop] -------

        /// <summary>
        /// calculates the field values at a series of 
        /// arbitary (x,y, z)-positions 
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in x-domain </param>
        /// <param name="gIn"> sampling grid of input field </param>
        /// <param name="xys"> set of target (x, y)-positions </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at (x,y, z)positions </returns>
        internal static VectorZ Propagate2D(double wavelength, Complex n,
            MatrixZ vIn, GridInfo2D gIn,
            ScatInfo2D xys, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (xys.Count != zs.Count)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initiaization
            VectorZ vOut = new(count: xys.Count);

            // defines loop operation
            Action<long> a = (i) =>
            {
                (double yi, double xi) = xys[i, false];
                vOut[i, false] = Propagate2D(wavelength: wavelength, n: n,
                    vIn: vIn, gIn: gIn,
                    x: xi, y: yi, z: zs[i, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop1D loop = new(operation: a,
                start: 0, end: vOut.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// calculates the field values at a series of 
        /// (x,y, z)-positions with separable x,y distributions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in x-domain </param>
        /// <param name="gIn"> sampling grid of input field </param>
        /// <param name="xys"> uniformly distributed target (x,y)-positions </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at (x,y, z)positions </returns>
        internal static MatrixZ Propagate2D(double wavelength, Complex n,
            MatrixZ vIn, GridInfo2D gIn,
            ScatInfoXY xys, MatrixD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (xys.Rows != zs.Rows || xys.Cols != zs.Cols)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initiaization
            MatrixZ vOut = new(rows: xys.Rows, cols: xys.Cols);

            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double xi, double yi) = xys[iRow, iCol, false];
                vOut[iRow, iCol, false] = Propagate2D(wavelength: wavelength, n: n,
                    vIn: vIn, gIn: gIn,
                    x: xi, y: yi, z: zs[iRow, iCol, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: xys.Rows, 
                colStart: 0, colEnd: xys.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return vOut;
        }

        /// <summary>
        /// calculates the field values at a series of 
        /// (x,y, z)-positions with uniform x,y distributions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vIn"> values of input field in x-domain </param>
        /// <param name="gIn"> sampling grid of input field </param>
        /// <param name="gTarget"> target grid of output field </param>
        /// <param name="zs"> z-distancees at all target grid locations </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> field values at (x,y, z)positions </returns>
        internal static MatrixZ Propagate2D(double wavelength, Complex n,
            MatrixZ vIn, GridInfo2D gIn,
            GridInfo2D gTarget, MatrixD zs,
            LoopMode loopMode = Defaults.LoopOption)
            => Propagate2D(wavelength: wavelength, n: n, vIn: vIn, gIn: gIn,
                xys: (ScatInfoXY)gTarget, zs: zs, loopMode: loopMode);

        /// <summary>
        /// calculates the field values a series of 
        /// arbitary (x,y, z)-positions 
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> sampling grid of both input field components </param>
        /// <param name="xys"> set of target (x, y)-positions </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> complex (Ex, Ey) field values at (x,y, z)-positions </returns>
        internal static (VectorZ, VectorZ) Propagate2D(double wavelength, Complex n,
            MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
            ScatInfo2D xys, VectorD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (xys.Count != zs.Count)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initiaization
            VectorZ vOutEx = new(count: xys.Count);
            VectorZ vOutEy = new(count: xys.Count);

            // defines loop operation
            Action<long> a = (i) =>
            {
                (double yi, double xi) = xys[i, false];
                (Complex vEx, Complex vEy) = Propagate2D(wavelength: wavelength, n: n,
                    vInEx: vInEx, vInEy: vInEy, gIn: gIn,
                    x: xi, y: yi, z: zs[i, false], 
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
                vOutEx[i, false] = vEx;
                vOutEx[i, false] = vEy;
            };
            Loop1D loop = new(operation: a, start: 0, end: xys.Count, step: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (vOutEx, vOutEy);
        }

        /// <summary>
        /// calculates the field values a series of 
        /// (x,y, z)-positions with separable x,y distributions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> sampling grid of both input field components </param>
        /// <param name="xys"> set of target (x, y)-positions </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> complex (Ex, Ey) field values at (x,y, z)-positions </returns>
        internal static (MatrixZ, MatrixZ) Propagate2D(double wavelength, Complex n,
            MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
            ScatInfoXY xys, MatrixD zs,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (xys.Rows != zs.Rows || xys.Cols != zs.Cols)
            { throw new ArgumentException($"Unequal number of input elements"); }
        
            // initialization
            MatrixZ vOutEx = new(rows: xys.Rows, cols: xys.Cols);
            MatrixZ vOutEy = new(rows: xys.Rows, cols: xys.Cols); ;

            // defines loop operation
            Action<long, long> a = (iRow, iCol) =>
            {
                (double xi, double yi) = xys[iRow, iCol, false];
                (Complex vEx, Complex vEy) = Propagate2D(wavelength: wavelength, n: n,
                    vInEx: vInEx, vInEy: vInEy, gIn: gIn,
                    x: xi, y: yi, z: zs[iRow, iCol, false],
                    loopMode: LoopMode.Sequential); // sets inner loop to sequential
                vOutEx[iRow, iCol, false] = vEx;
                vOutEy[iRow, iCol, false] = vEy;
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: xys.Rows,
                colStart: 0, colEnd: xys.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);

            // return
            return (vOutEx, vOutEy);
        }

        /// <summary>
        /// calculates the field values a series of 
        /// (x,y, z)-positions with uniform x,y distributions
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index @wavelength </param>
        /// <param name="vInEx"> input Ex field in k-domain </param>
        /// <param name="vInEy"> input Ey field in k-domain </param>
        /// <param name="gIn"> sampling grid of both input field components </param>
        /// <param name="gTarget"> target grid of output field components </param>
        /// <param name="zs"> z-distances at all the (x, y)-positions </param>
        /// <param name="loopMode"> computational option for loop </param>
        /// <returns> complex (Ex, Ey) field values at (x,y, z)-positions </returns>
        internal static (MatrixZ, MatrixZ) Propagate2D(double wavelength, Complex n,
            MatrixZ vInEx, MatrixZ vInEy, GridInfo2D gIn,
            GridInfo2D gTarget, MatrixD zs,
            LoopMode loopMode = Defaults.LoopOption)
            => Propagate2D(wavelength: wavelength, n: n, vInEx: vInEx, vInEy: vInEy, gIn: gIn,
                xys: (ScatInfoXY)gTarget, zs: zs, loopMode: loopMode);

        #endregion
        #region ------- 2D kernel [k-domain] -------

        /// <summary>
        /// Propagates a 2D field in the k-domain to a parallel plane at a given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method, with support for additional linear phase terms.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="v">Field values in k-domain (input/output).</param>
        /// <param name="nx">Vector of normalized kx values (nx = kx / k0).</param>
        /// <param name="ny">Vector of normalized ky values (ny = ky / k0).</param>
        /// <param name="nz">Matrix of normalized kz values (nz = kz / k0).</param>
        /// <param name="z">Propagation distance along the z direction.</param>
        /// <param name="cLinearX">Coefficient of the additional linear phase along the x direction added to the propagation kernel in k-domain.</param>
        /// <param name="cLinearY">Coefficient of the additional linear phase along the y direction added to the propagation kernel in k-domain.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        internal static void Propagate2D(double wavelength,
            ref MatrixZ v, VectorD nx, VectorD ny, MatrixZ nz,
            double z,
            double cLinearX = 0.0, double cLinearY = 0.0,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (v.Rows != nz.Rows || v.Cols != nz.Cols)
            { throw new ArgumentException($"Unequal number of input elements"); }

            Complex imOne = Complex.ImaginaryOne;
            double k0 = 2.0 * Math.PI / wavelength;
            MatrixZ values = v;

            if (cLinearX == 0.0 && cLinearY == 0.0)
            {
                void op(long iRow, long iCol)
                {
                    Complex nzi = nz[iRow, iCol, false];
                    values[iRow, iCol, false] *= Complex.Exp(imOne * k0 * nzi * z);
                }
                new Loop2D(op, 0, v.Rows, 0, v.Cols, 1, 1).Evaluate(loopMode);
            }
            else
            {
                void op(long iRow, long iCol)
                {
                    double nxi = nx[iCol, false];
                    double nyi = ny[iRow, false];
                    Complex nzi = nz[iRow, iCol, false];
                    values[iRow, iCol, false] *= Complex.Exp(imOne * k0 * (nzi * z + nxi * cLinearX + nyi * cLinearY));
                }
                new Loop2D(op, 0, v.Rows, 0, v.Cols, 1, 1).Evaluate(loopMode);
            }
        }


        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="v"> field values in k-domain (in/out) </param>
        /// <param name="nz"> normalized kx => nz = kz/k0 </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        internal static void Propagate2D(double wavelength,
            ref MatrixZ v, MatrixZ nz,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if(v.Rows != nz.Rows || v.Cols != nz.Cols)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initialization
            double k0 = 2.0 * Math.PI / wavelength;

            // defines loop operation
            MatrixZ values = v;
            Action<long, long> a = (iRow, iCol) =>
            {
                values[iRow, iCol, false] *= Complex.Exp(Complex.ImaginaryOne * k0 * nz[iRow, iCol, false] * z);
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: v.Rows,
                colStart: 0, colEnd: v.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="vEx"> Ex field in k-domain [in/out] </param>
        /// <param name="vEy"> Ey field in k-domain [in/out] </param>
        /// <param name="nz"> normalized kx => nz = kz/k0 </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        internal static void Propagate2D(double wavelength,
            ref MatrixZ vEx, ref MatrixZ vEy, MatrixZ nz,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if(vEx.Rows != nz.Rows || vEx.Cols != nz.Cols 
                || vEy.Rows != nz.Rows || vEy.Cols != nz.Cols)
            { throw new ArgumentException($"Unequal number of input elements"); }

            // initialization
            double k0 = 2.0 * Math.PI / wavelength;

            // defines loop operation
            MatrixZ valuesEx = vEx;
            MatrixZ valuesEy = vEy;
            Action<long, long> a = (iRow, iCol) =>
            {
                Complex p = Complex.Exp(Complex.ImaginaryOne * k0 * nz[iRow, iCol, false] * z);
                valuesEx[iRow, iCol, false] *= p;
                valuesEy[iRow, iCol, false] *= p;
            };
            Loop2D loop = new(operation: a,
                rowStart: 0, rowEnd: nz.Rows,
                colStart: 0, colEnd: nz.Cols,
                rowStep: 1, colStep: 1);
            loop.Evaluate(mode: loopMode);
        }

        #endregion

    }

}
