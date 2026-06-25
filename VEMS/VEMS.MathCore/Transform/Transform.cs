using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// transform methods
    /// </summary>
    public class Transform
    {
        #region DFT methods

        #region --------- 1D Methods ---------

        /// <summary>
        /// calculates 1D discrete Fourier transform result at desired index j
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="j"> desired output index j; can be non-integer number </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at index j </returns>
        public static Complex DFT(VectorZ x, double j, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -2.0 * Math.PI * Complex.ImaginaryOne;
            if (opt == DFTOption.Backward) { preFac *= -1.0; }
            return DFTKernel.Transform1D(preFac, x, j, scalFac, loopMode);
        }

        /// <summary>
        /// calculates 1D discrete Fourier transform result at desired index j
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="j"> desired output index j; can be non-integer number </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at index j </returns>
        public static Complex DFT(VectorD x, double j, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
            => DFT((VectorZ)x, j, opt, scalFac);

        /// <summary>
        /// performs discrete Fourier transform on a complex vector
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        public static VectorZ DFT(VectorZ x, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -2.0 * Math.PI * Complex.ImaginaryOne;
            if (opt == DFTOption.Backward) { preFac *= -1.0; }
            return DFTKernel.Transform1D(preFac, x, scalFac, loopMode);
        }

        /// <summary>
        /// performs discrete Fourier transform on a complex vector
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        public static VectorZ DFT(VectorD x, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
            => DFT((VectorZ)x, opt, scalFac, loopMode);

        /// <summary>
        /// calculates 1D discrete Fourier transform result 
        /// at desired position t
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="t"> desired position t </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at positoin t </returns>
        public static Complex DFT(VectorZ x, GridInfo1D grid,
            double t, DFTOption opt, LoopMode loopMode = Defaults.LoopOption)
        {
            double j = t / grid.Spacing;
            double scalFac = grid.Spacing / Math.Sqrt(2.0 * Math.PI);
            return DFT(x, j, opt, scalFac, loopMode);
        }

        /// <summary>
        /// calculates 1D discrete Fourier transform result 
        /// at desired position t
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="t"> desired position t </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at positoin t </returns>
        public static Complex DFT(VectorD x, GridInfo1D grid,
            double t, DFTOption opt, LoopMode loopMode = Defaults.LoopOption)
            => DFT((VectorZ)x, grid, t, opt, loopMode);

        /// <summary>
        /// performs discrete Fourier transform on a complex vector
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data and its sampling grid </returns>
        public static (VectorZ, GridInfo1D) DFT(VectorZ x, GridInfo1D grid, DFTOption opt,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -2.0 * Math.PI * Complex.ImaginaryOne;
            if (opt == DFTOption.Backward) { preFac *= -1.0; }
            double scalFac = grid.Spacing / Math.Sqrt(2.0 * Math.PI);
            VectorZ v = DFTKernel.Transform1D(preFac, x, scalFac, loopMode);
            GridInfo1D g = new(other: grid);
            g.GetConjugated(isForward: opt == DFTOption.Forward);
            //g.GetConjugated();
            return (v, g);
        }

        /// <summary>
        /// performs discrete Fourier transform on a complex vector
        /// </summary>
        /// <param name="x"> input complex vector x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data and its sampling grid </returns>
        public static (VectorZ, GridInfo1D) DFT(VectorD x, GridInfo1D grid, DFTOption opt,
            LoopMode loopMode = Defaults.LoopOption)
            => DFT((VectorZ)x, grid, opt, loopMode);

        #endregion
        #region --------- 2D Methods ---------

        /// <summary>
        /// calculates discrete Fourier transform result 
        /// at desired indices jy, jx
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="jy"> desired output index jy; can be non-integer number </param>
        /// <param name="jx"> desired output index jx; can be non-integer number </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at indices jy, jx </returns>
        public static Complex DFT(MatrixZ x, double jy, double jx, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -2.0 * Math.PI * Complex.ImaginaryOne;
            if (opt == DFTOption.Backward) { preFac *= -1.0; }
            return DFTKernel.Transform2D(preFac, x, jy, jx, scalFac, loopMode);
        }

        /// <summary>
        /// calculates discrete Fourier transform result 
        /// at desired indices jy, jx
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="jy"> desired output index jy; can be non-integer number </param>
        /// <param name="jx"> desired output index jx; can be non-integer number </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at indices jy, jx </returns>
        public static Complex DFT(MatrixD x, double jy, double jx, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
            => DFT((MatrixZ)x, jy, jx, opt, scalFac, loopMode);

        /// <summary>
        /// performs discrete Fourier transform on a complex matrix
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        public static MatrixZ DFT(MatrixZ x, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -2.0 * Math.PI * Complex.ImaginaryOne;
            if (opt == DFTOption.Backward) { preFac *= -1.0; }
            return DFTKernel.Transform2D(preFac, x, scalFac, loopMode);
        }

        /// <summary>
        /// performs discrete Fourier transform on a complex matrix
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="scalFac"> scaling factor, default is 1.0 </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        public static MatrixZ DFT(MatrixD x, DFTOption opt,
            double scalFac = 1.0, LoopMode loopMode = Defaults.LoopOption)
            => DFT((MatrixZ)x, opt, scalFac, loopMode);

        /// <summary>
        /// calculates 1D discrete Fourier transform result 
        /// at desired position (ty, tx)
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="ty"> desired position ty </param>
        /// <param name="tx"> desired position tx </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed result at position (ty, tx) </returns>
        public static Complex DFT(MatrixZ x, GridInfo2D grid,
            double ty, double tx, DFTOption opt, LoopMode loopMode = Defaults.LoopOption)
        {
            double jx = tx / grid.SpacingX;
            double jy = ty / grid.SpacingY;
            double scalFac = grid.SpacingX * grid.SpacingY / (2.0 * Math.PI);
            return DFT(x, jy, jx, opt, scalFac, loopMode);
        }

        /// <summary>
        /// calculates 1D discrete Fourier transform result 
        /// at desired position (ty, tx)
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="ty"> desired position ty </param>
        /// <param name="tx"> desired position tx </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <returns> transformed result at position (ty, tx) </returns>
        public static Complex DFT(MatrixD x, GridInfo2D grid,
            double ty, double tx, DFTOption opt, LoopMode loopMode = Defaults.LoopOption)
            => DFT((MatrixZ)x, grid, ty, tx, opt, loopMode);

        /// <summary>
        /// performs discrete Fourier transform on a complex matrix
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        public static (MatrixZ, GridInfo2D) DFT(MatrixZ x, GridInfo2D grid,
            DFTOption opt, LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -2.0 * Math.PI * Complex.ImaginaryOne;
            if (opt == DFTOption.Backward) { preFac *= -1.0; }
            double scalFac = grid.SpacingX * grid.SpacingY / (2.0 * Math.PI);
            MatrixZ v = DFTKernel.Transform2D(preFac, x, scalFac, loopMode);
            GridInfo2D g = new(other: grid);
            g.GetConjugated(isForward: opt == DFTOption.Forward);
            return (v, g);
        }

        /// <summary>
        /// performs discrete Fourier transform on a complex matrix
        /// </summary>
        /// <param name="x"> input complex matrix x </param>
        /// <param name="grid"> sampling grid for the input </param>
        /// <param name="opt"> transform option: forward or backward </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> transformed data </returns>
        public static (MatrixZ, GridInfo2D) DFT(MatrixD x, GridInfo2D grid,
            DFTOption opt, LoopMode loopMode = Defaults.LoopOption)
            => DFT((MatrixZ)x, grid, opt, loopMode);

        #endregion

        #endregion

        #region Piecewise DFT ...

        /// <summary>
        /// calculate forward Fourier series at a desired index j
        /// </summary>
        /// <param name="x"> input piecewise data </param>
        /// <param name="j"> desired index j </param>
        /// <returns> Fourier coefficient at index j </returns>
        public static Complex? ForwardTransform1D(Pwct1DCplxData x,
            long j)
        {
            // prefactor
            Complex preFac = -Complex.ImaginaryOne * 2.0 * Math.PI;
            return PDFTKernel.Transform1D(preFac, x, j);
        }


        /// <summary>
        /// calculate forward Fourier series for array of desired indices
        /// </summary>
        /// <param name="x"> input piecewise data </param>
        /// <param name="startIndex"> start index of the Fourier coefficients </param>
        /// <param name="numCoeff"> number of the Fourier coefficients </param>
        /// <returns> result Fourier series </returns>
        public static VectorZ? ForwardTransform1D(Pwct1DCplxData x,
            long startIndex, long numCoeff)
        {
            // prefactor
            Complex preFac = -Complex.ImaginaryOne * 2.0 * Math.PI;
            return PDFTKernel.Transform1D(preFac, x, startIndex, numCoeff);
        }


        /// <summary>
        /// calculate backward Fourier series for array of desired indices
        /// </summary>
        /// <param name="x"> input piecewise data </param>
        /// <param name="j"> desired index j </param>
        /// <returns> result Fourier series </returns>
        public static Complex? BackwardTransform1D(Pwct1DCplxData x,
            long j)
        {
            // prefactor
            Complex preFac = Complex.ImaginaryOne * 2.0 * Math.PI;
            return PDFTKernel.Transform1D(preFac, x, j);
        }


        /// <summary>
        /// calculate backward Fourier series for array of desired indices
        /// </summary>
        /// <param name="x"> input piecewise data </param>
        /// <param name="startIndex"> start index of the Fourier coefficients </param>
        /// <param name="numCoeff"> number of the Fourier coefficients </param>
        /// <returns> result Fourier series </returns>
        public static VectorZ? BackwardTransform1D(Pwct1DCplxData x,
            long startIndex, long numCoeff)
        {
            // prefactor
            Complex preFac = Complex.ImaginaryOne * 2.0 * Math.PI;
            return PDFTKernel.Transform1D(preFac, x, startIndex, numCoeff);
        }

        #region Piecewise 2D PDFT

        /// <summary>
        /// calculate forward Fourier series coefficient at desired indices (jy, jx)
        /// for 2D piecewise-constant data
        /// </summary>
        public static Complex ForwardTransform2D(
            Pwct2DCplxData x,
            long jy,
            long jx)
        {
            Complex preFac = -Complex.ImaginaryOne * 2.0 * Math.PI;
            return Pwct2DPDFTKernel.Transform2D(preFac, x, jy, jx);
        }

        /// <summary>
        /// calculate forward Fourier series coefficient at desired indices (jy, jx)
        /// for 2D piecewise-constant data
        /// </summary>
        public static Complex ForwardTransform2D(
            Pwct2DCplxData x,
            double jy,
            double jx)
        {
            Complex preFac = -Complex.ImaginaryOne * 2.0 * Math.PI;
            return Pwct2DPDFTKernel.Transform2D(preFac, x, jy, jx);
        }

        /// <summary>
        /// calculate forward Fourier series for a block of coefficients
        /// </summary>
        public static MatrixZ ForwardTransform2D(
            Pwct2DCplxData x,
            long startIndexY,
            long numCoeffY,
            long startIndexX,
            long numCoeffX,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = -Complex.ImaginaryOne * 2.0 * Math.PI;
            return Pwct2DPDFTKernel.Transform2D(
                preFac,
                x,
                startIndexY,
                numCoeffY,
                startIndexX,
                numCoeffX,
                1.0,
                loopMode);
        }

        /// <summary>
        /// calculate backward Fourier series coefficient at desired indices (jy, jx)
        /// for 2D piecewise-constant data
        /// </summary>
        public static Complex BackwardTransform2D(
            Pwct2DCplxData x,
            long jy,
            long jx)
        {
            Complex preFac = Complex.ImaginaryOne * 2.0 * Math.PI;
            return Pwct2DPDFTKernel.Transform2D(preFac, x, jy, jx);
        }

        /// <summary>
        /// calculate backward Fourier series for a block of coefficients
        /// </summary>
        public static MatrixZ BackwardTransform2D(
            Pwct2DCplxData x,
            long startIndexY,
            long numCoeffY,
            long startIndexX,
            long numCoeffX,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Complex preFac = Complex.ImaginaryOne * 2.0 * Math.PI;
            return Pwct2DPDFTKernel.Transform2D(
                preFac,
                x,
                startIndexY,
                numCoeffY,
                startIndexX,
                numCoeffX,
                1.0,
                loopMode);
        }

        #endregion

        #endregion

        #region FFT methods

        #region ===== 1D Methods =====

        #region ---- in-place ----

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref Vector<Complex> x,
            double scalFac = 1.0,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // cases ...
            switch (convention)
            {
                case FFTOptions.Convention.ZeroBased:
                    FFTKernel.Transform1DRaw(x: ref x,
                        scalFac: scalFac,
                        isForward: direction == FFTOptions.Direction.Forward);
                    break;
                case FFTOptions.Convention.ZeroCentered:
                    FFTKernel.Transform1D(x: ref x,
                        scalFac: scalFac,
                        isForward: direction == FFTOptions.Direction.Forward,
                        useBlockShift: conversion == FFTOptions.Conversion.DataShift,
                        copyByBlock: copyMode == FFTOptions.CopyMode.Block,
                        useParallelFor: loopMode == FFTOptions.LoopMode.Parallel);
                    break;
                default: goto case FFTOptions.Convention.ZeroCentered;
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="c"> linear phase coefficient (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref Vector<Complex> x,
            ref GridInfo1D grid, ref double c,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // cases ...
            switch (convention)
            {
                case FFTOptions.Convention.ZeroBased:
                    Printer.Error($"Zero-based convention not supported for GridData ...");
                    break;
                case FFTOptions.Convention.ZeroCentered:
                    FFTKernel.Transform1D(x: ref x,
                        scalFac: grid.Spacing / Math.Sqrt(2.0 * Math.PI),
                        isForward: direction == FFTOptions.Direction.Forward,
                        useBlockShift: conversion == FFTOptions.Conversion.DataShift,
                        copyByBlock: copyMode == FFTOptions.CopyMode.Block,
                        useParallelFor: loopMode == FFTOptions.LoopMode.Parallel);
                    // calculates additional constant phase shift factor
                    if (c != 0 && grid.Center != 0)
                    {
                        Complex addPhaseFac = Complex.Exp(Complex.ImaginaryOne * c * grid.Center);
                        //VMath.ScaleOnZ(x: ref x, a: addPhaseFac);
                        unsafe { Defaults.IBLAS.Scal(n: x.Count, a: &addPhaseFac, x: x.VPtr); }
                    }
                    // updates the grid for output
                    grid.GetConjugated(isForward:
                        direction == FFTOptions.Direction.Forward, ref c);
                    break;
                default: goto case FFTOptions.Convention.ZeroCentered;
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref Vector<Complex> x, ref GridInfo1D grid,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            double c = 0.0;
            FFT1D(ref x, ref grid, ref c, direction, convention, conversion, copyMode, loopMode);

            // output linear phase handling ...
            if (c != 0.0)
            {
                Func<double, Complex> linPhase = (u) => Complex.Exp(Complex.ImaginaryOne * c * u);
                VectorZ lPh = new Samp1DCplxFunc(f: linPhase).Sample(grid: grid);
                //VMath.MulZ(x: x, y: lPh, z: ref x);
                unsafe { Defaults.IVMF.Mul(n: x.Count, a: x.VPtr, b: lPh.VPtr, y: x.VPtr); }
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="d"> data (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref Grid1DCplxData d,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Vector<Complex> x = d.Values;
            GridInfo1D g = d.GridInfo;
            double c1 = d.Phase.C1;
            FFT1D(ref x, ref g, ref c1, direction, convention, conversion, copyMode, loopMode);
            d.Phase.C1 = c1; // set the VALUE manually
            d.GridInfo = g; // update the grid info
        }


        // wrappers below ...

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref VectorZ x,
            double scalFac = 1.0,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Vector<Complex> t = x;
            FFT1D(ref t, scalFac, direction, convention, conversion, copyMode, loopMode);
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="c"> linear phase coefficient (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref VectorZ x,
            ref GridInfo1D grid, ref double c,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)

        {
            Vector<Complex> t = x;
            FFT1D(ref t, ref grid, ref c, direction, convention, conversion, copyMode, loopMode);
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT1D(ref VectorZ x, ref GridInfo1D grid,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Vector<Complex> t = x;
            FFT1D(ref t, ref grid, direction, convention, conversion, copyMode, loopMode);
        }

        #region ... obsolete ...

        /// <summary>
        /// performs in-place raw FFT transform
        /// with zero-based convention
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="option"> forward or backward transform </param>
        [Obsolete]
        public static void FFT1DRaw(ref Vector<Complex> x,
            double scalFac = 1.0,
            FTOption option = FTOption.Forward)
            => FFTKernel.Transform1DRaw(ref x, scalFac,
                isForward: option == FTOption.Forward);

        /// <summary>
        /// performs in-place raw FFT transform
        /// with zero-based convention
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="option"> forward or backward transform </param>
        [Obsolete]
        public static void FFT1DRaw(ref VectorZ x,
            double scalFac = 1.0,
            FTOption option = FTOption.Forward)
        {
            Vector<Complex> t = x;
            FFT1DRaw(ref t, scalFac, option);
        }


        /// <summary>
        /// performs in-place FFT transform
        /// with zero-centered convention
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="c"> linear phase coefficient (in/out) </param>
        /// <param name="option"> forward or backward transform </param>
        [Obsolete]
        public static void FFT1D(ref Vector<Complex> x,
            ref GridInfo1D grid,
            ref double c,
            FTOption option = FTOption.Forward)
        {
            // data transform kernel
            FFTKernel.Transform1D(x: ref x,
                scalFac: grid.Spacing / Math.Sqrt(2.0 * Math.PI),
                isForward: option == FTOption.Forward);

            // updates the grid for output
            grid.GetConjugated(option, ref c);

            // calculates additional constant phase shift factor
            if (c != 0.0)
            {
                Complex addPhaseFac = Complex.Exp(Complex.ImaginaryOne * c * grid.Center);
                //VMath.ScaleOnZ(x: ref x, a: addPhaseFac);
                unsafe { Defaults.IBLAS.Scal(n: x.Count, a: &addPhaseFac, x: x.VPtr); }
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform [advanced version]
        /// with shift theorem considered, for both shift and linear phase
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="c"> linear phase coefficient (in/out) </param>
        /// <param name="option"> forward or backward transform </param>
        [Obsolete]
        public static void FFT1D(ref VectorZ x,
            ref GridInfo1D grid,
            ref double c,
            FTOption option = FTOption.Forward)
        {
            Vector<Complex> t = x;
            FFT1D(ref t, ref grid, ref c, option);
        }


        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="option"> forward or backward transform </param>
        [Obsolete]
        public static void FFT1D(ref Vector<Complex> x,
            ref GridInfo1D grid,
            FTOption option = FTOption.Forward)
        {
            double c = 0.0;
            FFT1D(ref x, ref grid, ref c, option);

            // output linear phase handling ...
            if (c != 0.0)
            {
                Func<double, Complex> linPhase = (u) => Complex.Exp(Complex.ImaginaryOne * c * u);
                VectorZ lPh = new Samp1DCplxFunc(f: linPhase).Sample(grid: grid);
                //VMath.MulZ(x: x, y: lPh, z: ref x);
                unsafe { Defaults.IVMF.Mul(n: x.Count, a: x.VPtr, b: lPh.VPtr, y: x.VPtr); }
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        [Obsolete]
        public static void FFT1D(ref VectorZ x, ref GridInfo1D grid,
            FTOption option = FTOption.Forward)
        {
            Vector<Complex> t = x;
            FFT1D(ref t, ref grid, option);
        }


        /// <summary>
        /// performs in-place 1D FFT transform [advanced version]
        /// with shift theorem considered, for both shift and linear phase
        /// </summary>
        /// <param name="d"> data (in/out) </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        [Obsolete]
        public static void FFT1D(ref Grid1DCplxData d,
            FTOption option = FTOption.Forward)
        {
            Vector<Complex> x = d.Values;
            GridInfo1D g = d.GridInfo;
            double c1 = d.Phase.C1;
            FFT1D(ref x, ref g, ref c1, option);
            d.Phase.C1 = c1; // set the VALUE manually
        }

        #endregion

        #endregion
        #region ---- out-place ----

        /// <summary>
        /// performs out-place 1D FFT transform
        /// </summary>
        /// <param name="xIn"> input data vector </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        /// <returns> output data vector </returns>
        public static Vector<Complex> FFT1D(Vector<Complex> xIn,
            double scalFac = 1.0,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // copies input data
            Vector<Complex> xOut = new(other: xIn, copyMode: ArrayCopyMode.Deep);
            // calls the in-place transform
            FFT1D(x: ref xOut, scalFac, direction, convention, conversion, copyMode, loopMode);
            // return
            return xOut;
        }

        /// <summary>
        /// performs out-place 1D FFT transform
        /// </summary>
        /// <param name="xIn"> input data vector </param>
        /// <param name="gIn"> sampling grid of input data </param>
        /// <param name="cIn"> input linear phase coefficient </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        /// <returns> output (data, grid, c) </returns>
        public static (Vector<Complex>, GridInfo1D, double) FFT1D(
            Vector<Complex> xIn,
            GridInfo1D gIn, double cIn,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // copies input parameters
            Vector<Complex> xOut = new(other: xIn, copyMode: ArrayCopyMode.Deep);
            GridInfo1D gOut = new(other: gIn);
            double cOut = cIn;
            // calls the in-place transform
            FFT1D(x: ref xOut, grid: ref gOut, c: ref cOut,
                direction, convention, conversion, copyMode, loopMode);
            // return
            return (xOut, gOut, cOut);
        }


        // wrappers below ...

        /// <summary>
        /// performs out-place 1D FFT transform
        /// </summary>
        /// <param name="xIn"> input data vector </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        /// <returns> output data vector </returns>
        public static VectorZ FFT1D(VectorZ xIn,
            double scalFac = 1.0,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // copies input data
            VectorZ xOut = new(other: xIn, deepCopy: true);
            // calls the in-place transform
            FFT1D(x: ref xOut, scalFac, direction, convention, conversion, copyMode, loopMode);
            // return
            return xOut;
        }

        /// <summary>
        /// performs out-place 1D FFT transform
        /// </summary>
        /// <param name="xIn"> input data vector </param>
        /// <param name="gIn"> sampling grid of input data </param>
        /// <param name="cIn"> input linear phase coefficient </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        /// <returns> output (data, grid, c) </returns>
        public static (VectorZ, GridInfo1D, double) FFT1D(VectorZ xIn,
            GridInfo1D gIn, double cIn,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // copies input parameters
            VectorZ xOut = new(other: xIn, deepCopy: true);
            GridInfo1D gOut = new(other: gIn);
            double cOut = cIn;
            // calls the in-place transform
            FFT1D(x: ref xOut, grid: ref gOut, c: ref cOut,
                direction, convention, conversion, copyMode, loopMode);
            // return
            return (xOut, gOut, cOut);
        }

        // ...

        #region ... obsolete ...

        /// <summary>
        /// performs out-place 1D FFT transform [advanced version]
        /// with shift theorem considered
        /// </summary>
        /// <param name="xIn"> input data vector x </param>
        /// <param name="gIn"> sampling grid of input data </param>
        /// <param name="cIn"> linear phase coefficient of input data </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        /// <returns> transform results (data, grid, linear phase coefficient) </returns>
        [Obsolete]
        public static (VectorZ, GridInfo1D, double) FFT1D(VectorZ xIn,
            GridInfo1D? gIn = null, double cIn = 0.0,
            FTOption option = FTOption.Forward)
        {
            // null case handling
            gIn ??= new(n: xIn.Count);

            // copies input parameters
            VectorZ xOut = new(other: xIn, deepCopy: true);
            GridInfo1D gOut = new(other: gIn);
            double cOut = cIn;

            // calls the in-place transform method
            FFT1D(x: ref xOut, grid: ref gOut, c: ref cOut,
                option: option);

            // return
            return (xOut, gOut, cOut);
        }

        /// <summary>
        /// performs out-place 1D FFT transform
        /// </summary>
        /// <param name="xIn"> input data vector x </param>
        /// <param name="gIn"> sampling grid of input data </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        /// <returns> transform results (data, grid) </returns>
        [Obsolete]
        public static (VectorZ, GridInfo1D) FFT1D(VectorZ xIn,
            GridInfo1D? gIn = null,
            FTOption option = FTOption.Forward)
        {
            // null case handling
            gIn ??= new(n: xIn.Count);

            // copies input parameters
            VectorZ xOut = new(other: xIn, deepCopy: true);
            GridInfo1D gOut = new(other: gIn);

            // calls the in-place transform method
            FFT1D(x: ref xOut, grid: ref gOut,
                option: option);

            // return
            return (xOut, gOut);
        }

        #endregion

        #endregion

        #endregion
        #region ===== 2D Methods =====

        #region ---- in-place ----

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref Matrix<Complex> x,
            double scalFac = 1.0,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // cases ...
            switch (convention)
            {
                case FFTOptions.Convention.ZeroBased:
                    FFTKernel.Transform2DRaw(x: ref x,
                        scalFac: scalFac,
                        isForward: direction == FFTOptions.Direction.Forward);
                    break;
                case FFTOptions.Convention.ZeroCentered:
                    FFTKernel.Transform2D(x: ref x,
                        scalFac: scalFac,
                        isForward: direction == FFTOptions.Direction.Forward,
                        useBlockShift: conversion == FFTOptions.Conversion.DataShift,
                        copyByBlock: copyMode == FFTOptions.CopyMode.Block,
                        useParallelFor: loopMode == FFTOptions.LoopMode.Parallel);
                    break;
                default: goto case FFTOptions.Convention.ZeroCentered;
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="cx"> linear phase coefficient along x-direction (in/out) </param>
        /// <param name="cy"> linear phase coefficient along y-direction (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref Matrix<Complex> x,
            ref GridInfo2D grid, ref double cx, ref double cy,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            // cases ...
            switch (convention)
            {
                case FFTOptions.Convention.ZeroBased:
                    Printer.Error($"Zero-based convention not supported for GridData ...");
                    break;
                case FFTOptions.Convention.ZeroCentered:
                    FFTKernel.Transform2D(x: ref x,
                        scalFac: grid.SpacingX * grid.SpacingY / (2.0 * Math.PI),
                        isForward: direction == FFTOptions.Direction.Forward,
                        useBlockShift: conversion == FFTOptions.Conversion.DataShift,
                        copyByBlock: copyMode == FFTOptions.CopyMode.Block,
                        useParallelFor: loopMode == FFTOptions.LoopMode.Parallel);
                    // calculates additional constant phase shift factor
                    if (cx != 0.0 || cy != 0.0)
                    {
                        Complex addPhaseFac = Complex.Exp(Complex.ImaginaryOne * (cy * grid.CenterY + cx * grid.CenterX));
                        //VMath.ScaleOnZ(x: ref x, a: addPhaseFac);
                        unsafe { Defaults.IBLAS.Scal(n: x.Count, a: &addPhaseFac, x: x.VPtr); }
                    }
                    // updates the grid for output
                    grid.GetConjugated(isForward:
                        direction == FFTOptions.Direction.Forward,
                        ref cx, ref cy);
                    break;
                default: goto case FFTOptions.Convention.ZeroCentered;
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref Matrix<Complex> x,
            ref GridInfo2D grid,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            double cx = 0.0;
            double cy = 0.0;
            FFT2D(ref x, ref grid, ref cx, ref cy, direction, convention, conversion, copyMode, loopMode);
            // output linear phase handling ...
            if (cx != 0.0 || cy != 0.0)
            {
                Func<double, double, Complex> linPhase = (u, v) => Complex.Exp(Complex.ImaginaryOne * (cx * u + cy * v));
                MatrixZ lPh = new Samp2DCplxFunc(f: linPhase).Sample(grid: grid);
                //VMath.MulZ(x: x, y: lPh, z: ref x);
                unsafe { Defaults.IVMF.Mul(n: x.Count, a: x.VPtr, b: lPh.VPtr, y: x.VPtr); }
            }
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="d"> data (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref Grid2DCplxData d,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Matrix<Complex> x = d.Values;
            GridInfo2D g = d.GridInfo;
            double c1x = d.Phase.C1x;
            double c1y = d.Phase.C1y;
            FFT2D(ref x, ref g, ref c1x, ref c1y,
                direction, convention, conversion, copyMode, loopMode);
            d.Phase.C1x = c1x; // set the VALUE manually
            d.Phase.C1y = c1y; // set the VALUE manually
        }


        // wrappers below ...

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> zero-centered or zero-based convention </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref MatrixZ x,
            double scalFac = 1.0,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Matrix<Complex> t = x;
            FFT2D(ref t, scalFac, direction, convention, conversion, copyMode, loopMode);
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="cx"> linear phase coefficient along x-direction (in/out) </param>
        /// <param name="cy"> linear phase coefficient along y-direction (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref MatrixZ x,
            ref GridInfo2D grid, ref double cx, ref double cy,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Matrix<Complex> t = x;
            FFT2D(ref t, ref grid, ref cx, ref cy, direction, convention, conversion, copyMode, loopMode);
        }

        /// <summary>
        /// performs in-place 1D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="direction"> forward or backward transform </param>
        /// <param name="convention"> only zero-centered convention is supported </param>
        /// <param name="conversion"> prefered conversion method, for zero-centered convention only </param>
        /// <param name="copyMode"> prefered copy mode, for DataShift case only </param>
        /// <param name="loopMode"> for-loop option, for element-wise copy or linear phase calculation </param>
        public static void FFT2D(ref MatrixZ x,
            ref GridInfo2D grid,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.Conversion conversion = FFTOptions.Conversion.DataShift,
            FFTOptions.CopyMode copyMode = FFTOptions.CopyMode.Block,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Matrix<Complex> t = x;
            FFT2D(ref t, ref grid, direction, convention, conversion, copyMode, loopMode);
        }

        #region ... obsolete ...

        /// <summary>
        /// performs in-place raw FFT transform
        /// with zero-based convention
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="option"> forward or backward transform </param>
        public static void FFT2DRaw(ref Matrix<Complex> x,
            double scalFac = 1.0,
            FTOption option = FTOption.Forward)
            => FFTKernel.Transform2DRaw(x: ref x,
                scalFac: scalFac,
                isForward: option == FTOption.Forward);

        /// <summary>
        /// performs in-place raw FFT transform
        /// with zero-based convention
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="option"> forward or backward transform </param>
        public static void FFT2DRaw(ref MatrixZ x,
            double scalFac = 1.0,
            FTOption option = FTOption.Forward)
        {
            Matrix<Complex> t = x;
            FFT2DRaw(ref t, scalFac, option);
        }


        /// <summary>
        /// performs in-place FFT transform
        /// with zero-centered convention
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="cx"> linear phase coefficient along x-direction (in/out) </param>
        /// <param name="cy"> linear phase coefficient along y-direction (in/out)</param>
        /// <param name="option"> forward or backward transform </param>
        public static void FFT2D(ref Matrix<Complex> x,
            ref GridInfo2D grid,
            ref double cx, ref double cy,
            FTOption option = FTOption.Forward)
        {
            // data transform kernel
            FFTKernel.Transform2D(x: ref x,
                scalFac: grid.SpacingX * grid.SpacingY / (2.0 * Math.PI),
                isForward: option == FTOption.Forward);

            // updates the grid for output
            grid.GetConjugated(option, ref cx, ref cy);

            // calculates additional constant phase shift factor
            if (cx != 0.0 || cy != 0.0)
            {
                Complex addPhaseFac = Complex.Exp(Complex.ImaginaryOne * (cy * grid.CenterY + cx * grid.CenterX));
                //VMath.ScaleOnZ(x: ref x, a: addPhaseFac);
                unsafe { Defaults.IBLAS.Scal(n: x.Count, a: &addPhaseFac, x: x.VPtr); }
            }
        }

        /// <summary>
        /// performs in-place FFT transform
        /// with zero-centered convention
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="cx"> linear phase coefficient along x-direction (in/out) </param>
        /// <param name="cy"> linear phase coefficient along y-direction (in/out)</param>
        /// <param name="option"> forward or backward transform </param>
        public static void FFT2D(ref MatrixZ x,
            ref GridInfo2D grid,
            ref double cx, ref double cy,
            FTOption option = FTOption.Forward)
        {
            Matrix<Complex> t = x;
            FFT2D(ref t, ref grid, ref cx, ref cy, option);
        }


        /// <summary>
        /// performs in-place FFT transform
        /// with zero-centered convention
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="option"> forward or backward transform </param>
        public static void FFT2D(ref Matrix<Complex> x,
            ref GridInfo2D grid,
            FTOption option = FTOption.Forward)
        {
            double cx = 0.0;
            double cy = 0.0;
            FFT2D(ref x, ref grid, ref cx, ref cy, option);

            // output linear phase handling ...
            if (cx != 0.0 || cy != 0.0)
            {
                Func<double, double, Complex> linPhase = (u, v) => Complex.Exp(Complex.ImaginaryOne * (cx * u + cy * v));
                MatrixZ lPh = new Samp2DCplxFunc(f: linPhase).Sample(grid: grid);
                //VMath.MulZ(x: x, y: lPh, z: ref x);
                unsafe { Defaults.IVMF.Mul(n: x.Count, a: x.VPtr, b: lPh.VPtr, y: x.VPtr); }
            }
        }

        /// <summary>
        /// performs in-place 2D FFT transform
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="grid"> sampling grid of the data (in/out) </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        public static void FFT2D(ref MatrixZ x,
            ref GridInfo2D grid,
            FTOption option = FTOption.Forward)
        {
            Matrix<Complex> t = x;
            FFT2D(ref t, ref grid, option);
        }


        /// <summary>
        /// performs in-place 1D FFT transform [advanced version]
        /// with shift theorem considered, for both shift and linear phase
        /// </summary>
        /// <param name="d"> data (in/out) </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        public static void FFT2D(ref Grid2DCplxData d,
            FTOption option = FTOption.Forward)
        {
            Matrix<Complex> x = d.Values;
            GridInfo2D g = d.GridInfo;
            double c1x = d.Phase.C1x;
            double c1y = d.Phase.C1y;
            FFT2D(ref x, ref g, ref c1x, ref c1y, option);
            d.Phase.C1x = c1x; // set the VALUE manually
            d.Phase.C1y = c1y; // set the VALUE manually
        }

        #endregion

        #endregion
        #region ---- out-place ----

        /// <summary>
        /// performs out-place 2D FFT transform [advanced version]
        /// with shift theorem considered
        /// </summary>
        /// <param name="xIn"> input data matrix </param>
        /// <param name="gIn"> sampling grid of the input data </param>
        /// <param name="cxIn"> input linear phase coefficient along x-direction </param>
        /// <param name="cyIn"> input linear phase coefficient along y-direction </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        /// <returns> transform results (data, grid, linear phase coefficient x/y) </returns>
        public static (MatrixZ, GridInfo2D, double, double) FFT2D(MatrixZ xIn,
            GridInfo2D? gIn = null, double cxIn = 0.0, double cyIn = 0.0,
            FTOption option = FTOption.Forward)
        {
            // null case handling
            gIn ??= new(rows: xIn.Rows, cols: xIn.Cols);

            // copies input parameters
            MatrixZ xOut = new(other: xIn, deepCopy: true);
            GridInfo2D gOut = new(other: gIn);
            double cxOut = cxIn;
            double cyOut = cyIn;

            // calls the in-place transform method
            FFT2D(x: ref xOut, grid: ref gOut, cx: ref cxOut, cy: ref cyOut,
                option: option);

            // return
            return (xOut, gOut, cxOut, cyOut);
        }

        /// <summary>
        /// performs out-place 2D FFT transform
        /// </summary>
        /// <param name="xIn"> input data matrix </param>
        /// <param name="gIn"> sampling grid of the input data </param>
        /// <param name="option"> Fourier transform option: forward or backward </param>
        /// <returns> transform results (data, grid) </returns>
        public static (MatrixZ, GridInfo2D) FFT2D(MatrixZ xIn,
            GridInfo2D? gIn = null,
            FTOption option = FTOption.Forward)
        {
            // null case handling
            gIn ??= new(rows: xIn.Rows, cols: xIn.Cols);

            // copies input parameters
            MatrixZ xOut = new(other: xIn, deepCopy: true);
            GridInfo2D gOut = new(other: gIn);

            // calls the in-place transform method
            FFT2D(x: ref xOut, grid: ref gOut,
                option: option);

            // return
            return (xOut, gOut);
        }

        #endregion

        #endregion

        #endregion

        #region FFS (fast Fourier series) methods

        #region ===== 1D Methods =====

        #region ---- in-place ----

        /// <summary>
        /// calculates in-place fast Fourier series
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="isForward"> forward or backward transform </param>
        public static void FFS1D(ref Vector<Complex> x,
            bool isForward = true)
            => FFTKernel.Transform1D(x: ref x,
                scalFac: 1.0 / x.Count,
                isForward: isForward);

        /// <summary>
        /// calculates in-place fast Fourier series
        /// </summary>
        /// <param name="x"> data vector x (in/out) </param>
        /// <param name="isForward"> forward or backward transform </param>
        public static void FFS1D(ref VectorZ x,
            bool isForward = true)
        {
            Vector<Complex> t = x;
            FFS1D(ref t, isForward);
        }

        #endregion
        #region ---- out-place ----

        // ...

        #endregion

        #endregion
        #region ===== 2D Methods =====

        #region ---- in-place ----

        /// <summary>
        /// calculates in-place fast Fourier series
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="isForward"> forward or backward transform </param>
        public static void FFS2D(ref Matrix<Complex> x,
            bool isForward = true)
            => FFTKernel.Transform2D(x: ref x,
                scalFac: 1.0 / (x.Rows * x.Cols),
                isForward: isForward);

        /// <summary>
        /// calculates in-place fast Fourier series
        /// </summary>
        /// <param name="x"> data matrix x (in/out) </param>
        /// <param name="isForward"> forward or backward transform </param>
        public static void FFS2D(ref MatrixZ x,
            bool isForward = true)
        {
            Matrix<Complex> t = x;
            FFS2D(x: ref t, isForward: isForward);
        }

        #endregion
        #region ---- out-place ----

        // ...

        #endregion

        #endregion

        #endregion

        #region CFT methods
        #region ===== 1D Methods =====
        /// <summary>
        /// Performs a 1D Chirp Fourier transform (CFT) on the input complex vector.
        /// Updates the sampling grid, quadratic phase coefficient and linear phase coefficient according to parameters.
        /// </summary>
        /// <param name="x">Input complex vector to be transformed (in/out).</param>
        /// <param name="grid">Sampling grid of the data (in/out).</param>
        /// <param name="c">Linear phase coefficient (in/out).</param>
        /// <param name="q">Quadratic phase coefficient (in/out).</param>
        /// <param name="direction">Transform direction: forward or backward.</param>
        /// <param name="convention">Transform convention: zero-centered or zero-based.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <remarks>
        /// This method assumes that the quadratic phase is always centered at the (0, 0).
        /// </remarks>
        public static void CFT1D(ref Vector<Complex> x,
            ref GridInfo1D grid,
            ref double c,
            ref double q,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            switch (convention)
            {
                case FFTOptions.Convention.ZeroBased:
                    Printer.Error($"Zero-based convention not supported for CFT ...");
                    break;
                case FFTOptions.Convention.ZeroCentered:
                    double scalFac = grid.Spacing / Math.Sqrt(2.0 * Math.PI);
                    // finds the center in the conjugated domain
                    double center = direction == FFTOptions.Direction.Forward ? +(2 * q * grid.Center + c) : -(2 * q * grid.Center + c);
                    // updates the cLinear in the conjugated domain and calculates the constant phase shift factor
                    Complex addPhaseFac = Complex.One;
                    if (q != 0.0)
                    {
                        addPhaseFac = Complex.Exp(-Complex.ImaginaryOne * c * c / (4 * q));
                        c = direction == FFTOptions.Direction.Forward ? +c / (2 * q) : -c / (2 * q);
                    }
                    else
                    {
                        addPhaseFac = Complex.Exp(Complex.ImaginaryOne * c * grid.Center);
                        c = direction == FFTOptions.Direction.Forward ? -grid.Center : +grid.Center;
                    }
                    CFTKernel.Transform1D(v: ref x,
                        scalFac: ref scalFac,
                        q: ref q,
                        isForward: direction == FFTOptions.Direction.Forward,
                        useParallelFor: loopMode == FFTOptions.LoopMode.Parallel);
                    // updates the grid for output
                    grid.Spacing = scalFac * Math.Sqrt(2.0 * Math.PI);
                    grid.Center = center;

                    // add additional constant phase shift factor
                    if (addPhaseFac != Complex.One)
                    {
                        //VMath.ScaleOnZ(x: ref x, a: addPhaseFac);
                        unsafe { Defaults.IBLAS.Scal(n: x.Count, a: &addPhaseFac, x: x.VPtr); }
                    }

                    break;
            }
        }
        /// <summary>
        /// Performs a 1D Chirp Fourier transform (CFT) on the input complex vector.
        /// Updates the sampling grid, linear phase coefficient, and quadratic phase coefficient according to parameters.
        /// </summary>
        /// <param name="x">Input complex vector to be transformed (in/out).</param>
        /// <param name="grid">Sampling grid of the data (in/out).</param>
        /// <param name="c">Linear phase coefficient (in/out).</param>
        /// <param name="q">Quadratic phase coefficient (in/out).</param>
        /// <param name="direction">Transform direction: forward or backward.</param>
        /// <param name="convention">Transform convention: zero-centered or zero-based.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <remarks>
        /// This method assumes that the quadratic phase is always centered at the (0, 0).
        /// </remarks>
        public static void CFT1D(ref VectorZ x,
            ref GridInfo1D grid,
            ref double c,
            ref double q,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            Vector<Complex> t = x;
            CFT1D(ref t, ref grid, ref c, ref q, direction, convention, loopMode);
        }
        #endregion
        #region ==== 2D methods ====
        /// <summary>
        /// Performs a 2D Chirped Fourier Transform (CFT) on the input matrix with specified quadratic phase parameters
        /// and updates the associated grid and phase parameters.
        /// </summary>
        /// <param name="x">The complex matrix to be transformed.</param>
        /// <param name="grid">The 2D grid information associated with the matrix.</param>
        /// <param name="cx">The linear phase parameter in the x-direction, updated during the transform.</param>
        /// <param name="cy">The linear phase parameter in the y-direction, updated during the transform.</param>
        /// <param name="q">The quadratic phase parameter in the x-direction, updated during the transform.</param>
        /// <param name="w">The cross-term quadratic phase parameter, updated during the transform.</param>
        /// <param name="p">The quadratic phase parameter in the y-direction, updated during the transform.</param>
        /// <param name="direction">The direction of the transform (forward or inverse).</param>
        /// <param name="convention">The convention for the Fourier transform (zero-centered or zero-based).</param>
        /// <param name="loopMode">Specifies whether to use sequential or parallel processing.</param>
        public static void CFT2D(ref MatrixZ x,
            ref GridInfo2D grid,
            ref double cx, ref double cy,
            ref double q, ref double w, ref double p,
            FFTOptions.Direction direction = FFTOptions.Direction.Forward,
            FFTOptions.Convention convention = FFTOptions.Convention.ZeroCentered,
            FFTOptions.LoopMode loopMode = FFTOptions.LoopMode.Sequential)
        {
            LoopMode lm = loopMode == FFTOptions.LoopMode.Parallel ? LoopMode.Parallel : LoopMode.Sequential;
            double theta = 0.0;
            switch (convention)
            {
                case FFTOptions.Convention.ZeroBased:
                    Printer.Error($"Zero-based convention not supported for CFT ...");
                    break;
                case FFTOptions.Convention.ZeroCentered:
                    if (w != 0.0)
                    {
                        theta = q == p ? Math.PI / 4.0 : Math.Atan2(w, q - p) / 2;
                        RotateInPlane.Rotate(ref x, ref grid, ref cx, ref cy, -(theta / Math.PI * 180.0), lm);
                        double q_ = q * Math.Cos(theta) * Math.Cos(theta) + w * Math.Cos(theta) * Math.Sin(theta) + p * Math.Sin(theta) * Math.Sin(theta);
                        p = q * Math.Sin(theta) * Math.Sin(theta) - w * Math.Cos(theta) * Math.Sin(theta) + p * Math.Cos(theta) * Math.Cos(theta);
                        q = q_;
                    }
                    double scalFacX = grid.SpacingX / Math.Sqrt(2.0 * Math.PI);
                    double scalFacY = grid.SpacingY / Math.Sqrt(2.0 * Math.PI);
                    // find the center in the conjugated domain
                    double centerX = direction == FFTOptions.Direction.Forward ?
                        +(2 * q * grid.CenterX + cx) :
                        -(2 * q * grid.CenterX + cx);
                    double centerY = direction == FFTOptions.Direction.Forward ?
                        +(2 * p * grid.CenterY + cy) :
                        -(2 * p * grid.CenterY + cy);

                    if (Math.Abs(q) < Math.PI / (grid.Cols * grid.SpacingX * grid.SpacingX) || Math.Abs(p) < Math.PI / (grid.Rows * grid.SpacingY * grid.SpacingY))
                        Printer.WriteLine("The Quadratic Parameters may not be suitable for Chirped Fourier Transform");
                    // updates the cLinear in the conjugated domain and calculates the constant phase shift factors
                    Complex addPhaseFac = Complex.One;
                    if (q != 0.0 && p != 0.0)
                    {
                        addPhaseFac = Complex.Exp(-Complex.ImaginaryOne * (cx * cx / (4.0 * q) + cy * cy / (4.0 * p)));
                        cx = direction == FFTOptions.Direction.Forward ?
                            +cx / (2 * q) :
                            -cx / (2 * q);
                        cy = direction == FFTOptions.Direction.Forward ?
                            +cy / (2 * p) :
                            -cy / (2 * p);
                    }
                    else if (q != 0.0)
                    {
                        addPhaseFac = Complex.Exp(-Complex.ImaginaryOne * (cx * cx / (4 * q) - cy * grid.CenterY));
                        cx = direction == FFTOptions.Direction.Forward ?
                            +cx / (2 * q) :
                            -cx / (2 * q);
                        cy = direction == FFTOptions.Direction.Forward ?
                             -grid.CenterY :
                            +grid.CenterY;
                    }
                    else if (p != 0)
                    {
                        addPhaseFac = Complex.Exp(Complex.ImaginaryOne * (cx * grid.CenterX - cy * cy / (4 * p)));
                        cx = direction == FFTOptions.Direction.Forward ?
                            -grid.CenterX :
                            +grid.CenterX;
                        cy = direction == FFTOptions.Direction.Forward ?
                            +cy / (2 * p) :
                            -cy / (2 * p);
                    }
                    else
                    {
                        addPhaseFac = Complex.Exp(Complex.ImaginaryOne * (cx * grid.CenterX + cy * grid.CenterY));
                        cx = direction == FFTOptions.Direction.Forward ?
                            -grid.CenterX :
                            +grid.CenterX;
                        cy = direction == FFTOptions.Direction.Forward ?
                            -grid.CenterY :
                            +grid.CenterY;
                    }

                    Matrix<Complex> v = x;
                    CFTKernel.Transform2D(v: ref v,
                        scalFacX: ref scalFacX,
                        scalFacY: ref scalFacY,
                        q: ref q,
                        p: ref p,
                        isForward: direction == FFTOptions.Direction.Forward,
                        useParallelFor: lm == LoopMode.Parallel
                        );


                    // updates the grid for output
                    grid.SpacingX = scalFacX * Math.Sqrt(2.0 * Math.PI);
                    grid.SpacingY = scalFacY * Math.Sqrt(2.0 * Math.PI);
                    grid.CenterX = centerX;
                    grid.CenterY = centerY;
                    // add additional phase shift factor
                    if (addPhaseFac != Complex.One)
                    {
                        unsafe { Defaults.IBLAS.Scal(n: x.Count, a: &addPhaseFac, x: x.VPtr); }
                        //VMath.ScaleOn(x: ref v, a: addPhaseFac);
                    }
                    break;
            }

            if (w != 0)
            {
                theta = -theta;
                RotateInPlane.Rotate(ref x, ref grid, ref cx, ref cy, -(theta / Math.PI * 180.0), lm);
                double q_ = q * Math.Cos(theta) * Math.Cos(theta) + p * Math.Sin(theta) * Math.Sin(theta);
                double p_ = q * Math.Sin(theta) * Math.Sin(theta) + p * Math.Cos(theta) * Math.Cos(theta);
                w = -2 * q * Math.Sin(theta) * Math.Cos(theta) + 2 * p * Math.Sin(theta) * Math.Cos(theta);
                q = q_;
                p = p_;
            }
        }
        #endregion
        #endregion
    }



    /// <summary>
    /// options for Fourier transform
    /// for both DFT and FFT
    /// </summary>
    [Obsolete]
    public enum FTOption
    {
        /// <summary>
        /// forward transform
        /// </summary>
        Forward,

        /// <summary>
        /// backward transform
        /// </summary>
        Backward
    }

}
