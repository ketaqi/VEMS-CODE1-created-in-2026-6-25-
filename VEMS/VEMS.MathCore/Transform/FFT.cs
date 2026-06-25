using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{
    /// <summary>
    /// collection of FFT-related optinos
    /// </summary>
    public struct FFTOptions
    {
        /// <summary>
        /// direction of the transform
        /// </summary>
        public enum Direction
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

        /// <summary>
        /// zero-convention
        /// </summary>
        public enum Convention
        {
            /// <summary>
            /// zero-centered: central value at index zero
            /// </summary>
            ZeroCentered,

            /// <summary>
            /// zero-based: first value at index zero
            /// </summary>
            ZeroBased
        }

        /// <summary>
        /// for zero-centered convention, specifies the preferred conversion method
        /// <para> option #1: conversion by linear phase </para>
        /// <para> option #2: conversion by data shift </para>
        /// </summary>
        public enum Conversion
        {
            /// <summary>
            /// linear phase: this is a generally valid method
            /// </summary>
            LinearPhase,

            /// <summary>
            /// data shift: this only works for even number of samples
            /// </summary>
            DataShift
        }

        /// <summary>
        /// for BlockShift, specifies the copy mode
        /// <para> option #1: copy by block(s) via BLAS </para>
        /// <para> option #2: copy by element via for loop </para>
        /// </summary>
        public enum CopyMode
        {
            /// <summary>
            /// copy by block via BLAS
            /// </summary>
            Block,

            /// <summary>
            /// copy by element via for-loop
            /// </summary>
            Element
        }

        /// <summary>
        /// for-loop mode options
        /// <para> for BlockShift case, applies when element-wise copy is used </para>
        /// <para> for LinearPhase case, applies for the linear phase calculation </para>
        /// </summary>
        public enum LoopMode
        {
            /// <summary>
            /// sequential for-loop
            /// </summary>
            Sequential,
            
            /// <summary>
            /// parallel for-loop
            /// </summary>
            Parallel
        }

    }




    /// <summary>
    /// FFT factory
    /// </summary>
    internal class FFTFactory
    {

        internal IFFT iFFT { get; set; }

        internal IVMF iVMF { get; set; }

        internal IBLAS iBLAS { get; set; }

        internal FFTFactory()
        {
            iFFT = Defaults.IFFT;
            iVMF = Defaults.IVMF;
            iBLAS = Defaults.IBLAS;
        }
    }


    /// <summary>
    /// FFT kernels for uniformly sampled data
    /// </summary>
	internal class FFTKernel
	{
        private static FFTFactory factory = new ();

        #region ---- helpers ----

        /// <summary>
        /// multiply phase term onto a complex array
        /// </summary>
        /// <param name="phi"> additional phase array </param>
        /// <param name="x"> complex array (to be overwritten) </param>
        /// <param name="realPart"> real part array </param>
        /// <param name="imagPart"> imaginary part array </param>
        /// <param name="abs"> absolute value array </param>
        /// <param name="arg"> argument array </param>
        private static void MultiplyPhase(DenseArrayBase<double> phi,
            ref DenseArrayBase<Complex> x,
            ref DenseArrayBase<double> realPart, ref DenseArrayBase<double> imagPart,
            ref DenseArrayBase<double> abs, ref DenseArrayBase<double> arg)
        {
            // consistency check
            // ...

            // gets real & imaginary parts
            factory.iVMF.RealImagParts(n: x.Count, a: x, re: ref realPart, im: ref imagPart);
            // converts to abs & arg
            factory.iVMF.HypotD(n: x.Count, a: realPart, b: imagPart, y: ref abs);
            factory.iVMF.Atan2D(n: x.Count, a: imagPart, b: realPart, y: ref arg);
            // adds phase
            factory.iVMF.AddD(n: x.Count, a: phi, b: arg, y: ref arg);
            // converts back
            factory.iVMF.SinCosD(n: arg.Count, a: arg, sin: ref imagPart, cos: ref realPart);
            factory.iVMF.MulD(n: abs.Count, a: abs, b: realPart, y: ref realPart);
            factory.iVMF.MulD(n: abs.Count, a: abs, b: imagPart, y: ref imagPart);
            // makes complex
            factory.iVMF.Modify(n: x.Count, re: realPart, im: imagPart, y: ref x);
        }

        /// <summary>
        /// multiply phase term onto a complex array
        /// </summary>
        /// <param name="phi"> additional phase array </param>
        /// <param name="x"> complex array (to be overwritten) </param>
        /// <param name="realPart"> real part array </param>
        /// <param name="imagPart"> imaginary part array </param>
        /// <param name="abs"> absolute value array </param>
        /// <param name="arg"> argument array </param>
        private static void MultiplyPhase(Vector<double> phi,
            ref Vector<Complex> x,
            ref Vector<double> realPart, ref Vector<double> imagPart,
            ref Vector<double> abs, ref Vector<double> arg)
        {
            // consistency check
            // ...

            // gets real & imaginary parts
            factory.iVMF.RealImagParts(n: x.Count, a: x, re: ref realPart, im: ref imagPart);
            // converts to abs & arg
            factory.iVMF.HypotD(n: x.Count, a: realPart, b: imagPart, y: ref abs);
            factory.iVMF.Atan2D(n: x.Count, a: imagPart, b: realPart, y: ref arg);
            // adds phase
            factory.iVMF.AddD(n: x.Count, a: phi, b: arg, y: ref arg);
            // converts back
            factory.iVMF.SinCosD(n: arg.Count, a: arg, sin: ref imagPart, cos: ref realPart);
            factory.iVMF.MulD(n: abs.Count, a: abs, b: realPart, y: ref realPart);
            factory.iVMF.MulD(n: abs.Count, a: abs, b: imagPart, y: ref imagPart);
            // makes complex
            factory.iVMF.Modify(n: x.Count, re: realPart, im: imagPart, y: ref x);
        }

        /// <summary>
        /// multiply phase term onto a complex array
        /// </summary>
        /// <param name="phi"> additional phase array </param>
        /// <param name="x"> complex array (to be overwritten) </param>
        /// <param name="realPart"> real part array </param>
        /// <param name="imagPart"> imaginary part array </param>
        /// <param name="abs"> absolute value array </param>
        /// <param name="arg"> argument array </param>
        private static void MultiplyPhase(Matrix<double> phi,
            ref Matrix<Complex> x,
            ref Matrix<double> realPart, ref Matrix<double> imagPart,
            ref Matrix<double> abs, ref Matrix<double> arg)
        {
            // consistency check
            // ...

            // gets real & imaginary parts
            factory.iVMF.RealImagParts(n: x.Count, a: x, re: ref realPart, im: ref imagPart);
            // converts to abs & arg
            factory.iVMF.HypotD(n: x.Count, a: realPart, b: imagPart, y: ref abs);
            factory.iVMF.Atan2D(n: x.Count, a: imagPart, b: realPart, y: ref arg);
            // adds phase
            factory.iVMF.AddD(n: x.Count, a: phi, b: arg, y: ref arg);
            // converts back
            factory.iVMF.SinCosD(n: arg.Count, a: arg, sin: ref imagPart, cos: ref realPart);
            factory.iVMF.MulD(n: abs.Count, a: abs, b: realPart, y: ref realPart);
            factory.iVMF.MulD(n: abs.Count, a: abs, b: imagPart, y: ref imagPart);
            // makes complex
            factory.iVMF.Modify(n: x.Count, re: realPart, im: imagPart, y: ref x);
        }


        /// <summary>
        /// data block shift for periodic data
        /// </summary>
        /// <param name="x"> periodic data whose blocks are to be shifted </param>
        /// <param name="n"> number of elements that defines the shift </param>
        /// <param name="copyByBlock"> whether to use block copy; default is true </param>
        /// <param name="useParallelFor"> for element-wise copy, whether to use parallel for loop </param>
        private unsafe static void PeriodicShift(ref Vector<Complex> x, long n,
            bool copyByBlock = true,
            bool useParallelFor = true)
        {
            if (x.Count == 0) { return; }

            // normalizes the shift to [0, x.Count)
            n = ((n % x.Count) + x.Count) % x.Count; 
            if (n == 0) { return; }

            // creates a temporary buffer for block copy
            Vector<Complex> s = new (other: x, copyMode: ArrayCopyMode.Deep);
            // using BLAS copy
            if (copyByBlock)
            {
                // copies the last (n) elements to the front
                factory.iBLAS.Copy(n: n, 
                    x: (Complex*)s.VPtr + (x.Count - n),
                    y: (Complex*)x.VPtr + 0, 
                    incx: 1, incy: 1);

                // then, copies the first (x.Count - n) elements to the end
                factory.iBLAS.Copy(n: x.Count - n, 
                    x: (Complex*)s.VPtr + 0, 
                    y: (Complex*)x.VPtr + n, 
                    incx: 1, incy: 1);
            }
            // using for loop
            else
            {
                Vector<Complex> t = x;
                Action<long> a = (i) => { t[(i + n) % t.Count, false] = s[i, false]; };
                Loop1D loop = new(operation: a, start: 0, end: x.Count);
                loop.Evaluate(mode: useParallelFor ? LoopMode.Parallel : LoopMode.Sequential);
                //for (long i = 0; i < x.Count; i++)
                //{ x[(i + n) % x.Count, false] = t[i, false]; }
            }
        }

        /// <summary>
        /// Periodic (circular) shift of a 2D complex array.
        /// </summary>
        /// <param name="x">2D array to shift (in-place)</param>
        /// <param name="nRow">Shift in rows (down if positive, up if negative)</param>
        /// <param name="nCol">Shift in columns (right if positive, left if negative)</param>
        /// <param name="copyByBlock"> whether to use block copy; default is true </param>
        /// <param name="useParallelFor"> for element-wise copy, whether to use parallel for loop </param>
        private unsafe static void PeriodicShift(ref Matrix<Complex> x, 
            long nRow, long nCol,
            bool copyByBlock = true,
            bool useParallelFor = true)
        {
            if (x.Rows == 0 || x.Cols == 0) { return; }

            // normalize shifts to [0, x.Rows) and [0, x.Cols)
            nRow = ((nRow % x.Rows) + x.Rows) % x.Rows;
            nCol = ((nCol % x.Cols) + x.Cols) % x.Cols;
            if (nRow == 0 && nCol == 0) { return; }

            // Temporary buffer for block copy
            Matrix<Complex> s = new (other: x, copyMode: ArrayCopyMode.Deep);
            // using BLAS copy
            if (copyByBlock) 
            {
                // loop over each row
                for (long iRow = 0; iRow < x.Rows; iRow++)
                {
                    long tRow = (nRow + iRow) % x.Rows;
                    // copies the last (nCol) elements to the front
                    // and moves to the target row (shift by nRow)
                    factory.iBLAS.Copy(n: nCol,
                        x: (Complex*)s.VPtr + iRow * x.Cols + (x.Cols - nCol),
                        y: (Complex*)x.VPtr + tRow * x.Cols + 0,
                        incx: 1, incy: 1);

                    // then, copies the first (x.Cols - nCol) elements to the end
                    // and also moves to the target row (shift by nRow)
                    factory.iBLAS.Copy(n: x.Cols - nCol,
                        x: (Complex*)s.VPtr + iRow * x.Cols + 0,
                        y: (Complex*)x.VPtr + tRow * x.Cols + nCol, 
                        incx: 1, incy: 1);
                }
            }
            // using for loop
            else
            {
                Matrix<Complex> t = x;
                Action<long, long> a = (iRow, iCol) =>
                {
                    long srcRow = (iRow + nRow) % t.Rows;
                    long srcCol = (iCol + nCol) % t.Cols;
                    t[iRow, iCol, false] = s[srcRow, srcCol, false];
                };
                Loop2D loop = new(operation: a, rowStart: 0, rowEnd: x.Rows,
                    colStart: 0, colEnd: x.Cols);
                loop.Evaluate(mode: useParallelFor ? LoopMode.Parallel : LoopMode.Sequential);

                //for (long iRow = 0; iRow < x.Rows; iRow++)
                //{
                //    long srcRow = (iRow + nRow) % x.Rows;
                //    for (long iCol = 0; iCol < x.Cols; iCol++)
                //    {
                //        long srcCol = (iCol + nCol) % x.Cols;
                //        x[iRow, iCol, false] = t[srcRow, srcCol, false];
                //    }
                //}
            }
        }

        #endregion
        #region ---- 1D ----

        /// <summary>
        /// performs in-place raw FFT transform
        /// with zero-based convention
        /// </summary>
        /// <param name="x"> input/output data </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="isForward"> forward or backward transform </param>
        internal static void Transform1DRaw(ref Vector<Complex> x,
            double scalFac = 1.0, 
            bool isForward = true)
        {
            // initializes descriptor
            IntPtr desc = new ();
            _ = factory.iFFT.DftiCreateDescriptor(desc: ref desc,
                precision: FFTConfigValue.DOUBLE, // precision
                domain: FFTConfigValue.COMPLEX, // domain
                dimension: 1, // dimension
                length: x.Count);

            // sets scale factor
            _ = factory.iFFT.DftiSetValue(desc: desc,
                config_param: isForward ? FFTConfigParam.FORWARD_SCALE 
                : FFTConfigParam.BACKWARD_SCALE,
                config_val: scalFac);

            // commits the descriptor
            _ = factory.iFFT.DftiCommitDescriptor(desc);

            // performs in-place forward transform
            _ = isForward ? factory.iFFT.DftiComputeForward(desc, x) 
                : factory.iFFT.DftiComputeBackward(desc, x);

            // free the descriptor
            _ = factory.iFFT.DftiFreeDescriptor(ref desc);
        }

        /// <summary>
        /// performs in-place FFT transform
        /// with zero-centered convention
        /// </summary>
        /// <param name="x"> input/output data </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="isForward"> forward or backward transform </param>
        /// <param name="useBlockShift"> use block shift (or not => linear phase multiplication) </param>
        /// <param name="copyByBlock"> for BlockShift only: copy by block (or not => element-wise copy) </param>
        /// <param name="useParallelFor"> enable parallal for-loop (for linear phase calculation or element-wise copy) </param>
        internal static void Transform1D(ref Vector<Complex> x,
            double scalFac = 1.0,
            bool isForward = true,
            bool useBlockShift = true,
            bool copyByBlock = true,
            bool useParallelFor = false)
        {
            // checks zero handling option with number of data elements
            if (x.Count %2 == 0) { useBlockShift = false; }

            // cases
            if (useBlockShift)
            {
                long shift = (x.Count - 1) / 2;
                // input data block shift
                PeriodicShift(ref x, -shift, copyByBlock, useParallelFor);
                // performs in-place raw transform
                Transform1DRaw(ref x, scalFac, isForward);
                // output data block shift
                PeriodicShift(ref x, shift, copyByBlock, useParallelFor);
            }
            else
            {
                #region linear phase definition
                double n = 0.5 * x.Count - 0.5;
                double dPhi = 2.0 * Math.PI * n / x.Count;
                if (!isForward) { dPhi *= -1.0; }
                Vector<double> phi = new(count: x.Count);
                double phiStart = -0.5 * n * dPhi;
                Action<long> a = (i) => { phi[i, false] = phiStart + i * dPhi; };
                Loop1D loop = new(operation: a, start: 0, end: phi.Count);
                loop.Evaluate(mode: useParallelFor? LoopMode.Parallel : LoopMode.Sequential);
                #endregion
                // auxillary variables
                Vector<double> realPart = new(count: x.Count);
                Vector<double> imagPart = new(count: x.Count);
                Vector<double> abs = new(count: x.Count);
                Vector<double> arg = new(count: x.Count);
                // input data multiplication with phase term
                MultiplyPhase(phi, ref x, ref realPart, ref imagPart, ref abs, ref arg);
                // performs in-place raw transform
                Transform1DRaw(ref x, scalFac, isForward);
                // output data multiplication with phase term
                MultiplyPhase(phi, ref x, ref realPart, ref imagPart, ref abs, ref arg);
            }
        }

        #endregion
        #region ---- 2D ----

        /// <summary>
        /// performs in-place raw FFT transform
        /// with zero-based convention
        /// </summary>
        /// <param name="x"> input/output data </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="isForward"> forward or backward transform </param>
        internal static void Transform2DRaw(ref Matrix<Complex> x,
            double scalFac = 1.0,
            bool isForward = true)
        {
            // initializes descriptor
            IntPtr desc = new ();
            _ = factory.iFFT.DftiCreateDescriptor(desc: ref desc,
                precision: FFTConfigValue.DOUBLE, // precision
                domain: FFTConfigValue.COMPLEX, // domain
                dimension: 2, // dimension
                lengths: [x.Rows, x.Cols]);

            // sets scale factor
            _ = factory.iFFT.DftiSetValue(desc: desc,
                config_param: isForward ? FFTConfigParam.FORWARD_SCALE 
                : FFTConfigParam.BACKWARD_SCALE,
                config_val: scalFac);
            
            // commits the descriptor
            _ = factory.iFFT.DftiCommitDescriptor(desc);
            
            // performs in-place forward transform
            _ = isForward ? factory.iFFT.DftiComputeForward(desc, x) 
                : factory.iFFT.DftiComputeBackward(desc, x);
            
            // free the descriptor
            _ = factory.iFFT.DftiFreeDescriptor(ref desc);
        }

        /// <summary>
        /// performs in-place FFT transform
        /// </summary>
        /// <param name="x"> input/output data </param>
        /// <param name="scalFac"> additional scaling factor </param>
        /// <param name="isForward"> forward or backward transform </param>
        /// <param name="useBlockShift"> use block shift (or not => linear phase multiplication) </param>
        /// <param name="copyByBlock"> for BlockShift only: copy by block (or not => element-wise copy) </param>
        /// <param name="useParallelFor"> enable parallal for-loop (for linear phase calculation or element-wise copy) </param>
        internal static void Transform2D(ref Matrix<Complex> x,
            double scalFac = 1.0,
            bool isForward = true,
            bool useBlockShift = true,
            bool copyByBlock = true,
            bool useParallelFor = true)
        {
            // checks zero handling option with number of data elements
            if (x.Count % 2 == 0) { useBlockShift = false; }

            // cases
            if (useBlockShift)
            {
                long shiftRows = (x.Rows - 1) / 2;
                long shiftCols = (x.Cols - 1) / 2;
                // input data block shift
                PeriodicShift(ref x, -shiftRows, -shiftCols, copyByBlock, useParallelFor);
                // performs in-place raw transform
                Transform2DRaw(ref x, scalFac, isForward);
                // output data block shift
                PeriodicShift(ref x, shiftRows, shiftCols, copyByBlock, useParallelFor);
            }
            else
            {
                #region linear phase definition
                double nRow = 0.5 * x.Rows - 0.5;
                double nCol = 0.5 * x.Cols - 0.5;
                double dPhiRow = 2.0 * Math.PI * nRow / x.Rows;
                double dPhiCol = 2.0 * Math.PI * nCol / x.Cols;
                if (!isForward) { dPhiRow *= -1.0; dPhiCol *= -1.0; }
                Matrix<double> phi = new(rows: x.Rows, cols: x.Cols);
                double phiStart = -0.5 * nRow * dPhiRow - 0.5 * nCol * dPhiCol;
                Action<long, long> a = (iRow, iCol) =>
                { phi[iRow, iCol, false] = phiStart + iRow * dPhiRow + iCol * dPhiCol; };
                Loop2D loop = new(operation: a, rowStart: 0, rowEnd: x.Rows, colStart: 0, colEnd: x.Cols);
                loop.Evaluate(mode: useParallelFor ? LoopMode.Parallel : LoopMode.Sequential);
                #endregion
                // auxillary variables
                Matrix<double> realPart = new(rows: x.Rows, cols: x.Cols);
                Matrix<double> imagPart = new(rows: x.Rows, cols: x.Cols);
                Matrix<double> abs = new(rows: x.Rows, cols: x.Cols);
                Matrix<double> arg = new(rows: x.Rows, cols: x.Cols);
                // input data multiplication with phase term
                MultiplyPhase(phi, ref x, ref realPart, ref imagPart, ref abs, ref arg);
                // performs in-place raw transform
                Transform2DRaw(ref x, scalFac, isForward);
                // output data multiplication with phase term
                MultiplyPhase(phi, ref x, ref realPart, ref imagPart, ref abs, ref arg);
            }
        }

        #endregion
    }

}
