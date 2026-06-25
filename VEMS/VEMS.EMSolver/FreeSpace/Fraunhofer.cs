using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Fraunhofer diffraction integral
    /// </summary>
    internal class Fraunhofer
    {

        #region ---- 1D kernel [x-domain] ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Fraunhofer diffraction integral method
        /// with paraxial and far-field approximation
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> real-part of the refractive index @wavelength </param>
        /// <param name="v"> field values in x-domain (in/out) </param>
        /// <param name="g"> sampling grid of input field (in/out) </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        internal static void Propagate1D(double wavelength, double nReal,
            ref VectorZ v, ref GridInfo1D g,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (v.Count != g.Count)
            { throw new ArgumentException($"Inconsistent input data size."); }

            // initialization
            double k0 = 2.0 * Math.PI / wavelength;
            double k = k0 * nReal;
            double kOver2D = k / (2.0 * z);

            // transforms to the k-domain
            Transform.FFT1D(x: ref v, grid: ref g,
                direction: FFTOptions.Direction.Forward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block,
                loopMode: FFTOptions.LoopMode.Sequential);
            
            // modifies target grid info (output directly in spatial domain)
            double dx = g.Spacing * z / k;
            g.Spacing = dx;
            g.Start = -0.5 * (g.Count * dx);
            
            // output factor, phase calculation and modification
            Complex imagOne = Complex.ImaginaryOne;
            Complex fac = -imagOne * k / z * Complex.Exp(imagOne * k * z);
            FieldCommons.AddQuadPhase(x: ref v, grid: g, a: kOver2D, 
                scalFac: fac, loopMode: loopMode);
        }

        ///// <summary>
        ///// propagates to a parallel plane at given distance
        ///// using the Fraunhofer diffraction integral method
        ///// with paraxial and far-field approximation
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of the refractive index @wavelength </param>
        ///// <param name="vEx"> Ex field component in x-domain (in/out) </param>
        ///// <param name="vEy"> Ey field component in x-domain (in/out) </param>
        ///// <param name="g"> sampling grid of both input field components (in/out) </param>
        ///// <param name="z"> propagation distance z </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        //internal static void Propagate1D(double wavelength, double nReal,
        //    ref VectorZ vEx, ref VectorZ vEy, ref GridInfo1D g,
        //    double z,
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    // consistency check
        //    if (vEx.Count != g.Count || vEy.Count != g.Count)
        //    { throw new ArgumentException($"Inconsistent input data size."); }

        //    // initialization
        //    double k0 = 2.0 * Math.PI / wavelength;
        //    double k = k0 * nReal;
        //    double kOver2D = k / (2.0 * z);
        //    // transforms to the k-domain
        //    GridInfo1D gEx = new(other: g);
        //    GridInfo1D gEy = new(other: g);
        //    Transform.FFT1D(x: ref vEx, grid: ref gEx, option: FTOption.Forward);
        //    Transform.FFT1D(x: ref vEy, grid: ref gEy, option: FTOption.Forward);
        //    // modifies target grid info (output directly in spatial domain)
        //    double dx = gEx.Spacing * z / k;
        //    g.Spacing = dx;
        //    g.Start = -0.5 * (g.Count * dx);
        //    // output factor, phase calculation and modification
        //    Complex fac = -Complex.ImaginaryOne * k / z
        //        * Complex.Exp(Complex.ImaginaryOne * k * z);
        //    FieldCommons.AddQuadPhase(x: ref vEx, grid: g, a: kOver2D, loopMode);
        //    FieldCommons.AddQuadPhase(x: ref vEy, grid: g, a: kOver2D, loopMode);
        //    VMath.ScaleOn(x: ref vEx, a: fac);
        //    VMath.ScaleOn(x: ref vEy, a: fac);
        //}

        #endregion
        #region ---- 2D kernel [x-domain] ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Fraunhofer diffraction integral method
        /// with paraxial and far-field approximation
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="nReal"> real-part of the refractive index @wavelength </param>
        /// <param name="v"> field values in x-domain (in/out) </param>
        /// <param name="g"> sampling grid of input field (in/out) </param>
        /// <param name="z"> propagation distance z </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        internal static void Propagate2D(double wavelength, double nReal,
            ref MatrixZ v, ref GridInfo2D g,
            double z,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // consistency check
            if (v.Rows != g.Rows || v.Cols != g.Cols)
            { throw new ArgumentException("Inconsistent input data size."); }

            // initialization
            double k0 = 2.0 * Math.PI / wavelength;
            double k = k0 * nReal;
            double kOver2D = k / (2.0 * z);
            
            // transform to the k-domain
            Transform.FFT2D(x: ref v, grid: ref g, 
                direction: FFTOptions.Direction.Forward,
                convention: FFTOptions.Convention.ZeroCentered,
                conversion: FFTOptions.Conversion.DataShift,
                copyMode: FFTOptions.CopyMode.Block,
                loopMode: FFTOptions.LoopMode.Sequential);

            // modify target grid info (output directly in spatial domain)
            double dx = g.SpacingX * z / k;
            double dy = g.SpacingY * z / k;
            g.SpacingY = dy;
            g.SpacingX = dx;
            g.StartY = -0.5 * (g.Rows * dy);
            g.StartX = -0.5 * (g.Cols * dx);
            // output factor, phase calculation and modification
            Complex imagOne = Complex.ImaginaryOne;
            Complex fac = -imagOne * k / z * Complex.Exp(imagOne * k * z);
            FieldCommons.AddQuadPhase(x: ref v, grid: g, 
                c2x: kOver2D, c2y: kOver2D,
                scalFac: fac, loopMode: loopMode);
        }

        ///// <summary>
        ///// propagates to a parallel plane at given distance
        ///// using the Fraunhofer diffraction integral method
        ///// with paraxial and far-field approximation
        ///// </summary>
        ///// <param name="wavelength"> wavelength in vacuum </param>
        ///// <param name="nReal"> real-part of the refractive index @wavelength </param>
        ///// <param name="vEx"> Ex field component in x-domain (in/out) </param>
        ///// <param name="vEy"> Ey field component in x-domain (in/out) </param>
        ///// <param name="g"> sampling grid of input field (in/out) </param>
        ///// <param name="z"> propagation distance z </param>
        ///// <param name="loopMode"> loop-computational mode options </param>
        //internal static void Propagate2D(double wavelength, double nReal,
        //    ref MatrixZ vEx, ref MatrixZ vEy, ref GridInfo2D g,
        //    double z, 
        //    LoopMode loopMode = Defaults.LoopOption)
        //{
        //    // consistency check
        //    if (vEx.Rows != g.Rows || vEx.Cols != g.Cols || vEy.Rows != g.Rows || vEy.Cols != g.Cols)
        //    { throw new ArgumentException("Inconsistent input data size."); }

        //    // initialization
        //    double k0 = 2.0 * Math.PI / wavelength;
        //    double k = k0 * nReal;
        //    double kOver2D = k / (2.0 * z);
        //    // transform to the k-domain
        //    GridInfo2D gEx = new(other: g);
        //    GridInfo2D gEy = new(other: g); 
        //    Transform.FFT2D(x: ref vEx, grid: ref gEx, option: FTOption.Forward);
        //    Transform.FFT2D(x: ref vEy, grid: ref gEy, option: FTOption.Forward);
        //    // modify target grid info (output directly in spatial domain)
        //    double dx = gEx.SpacingX * z / k;
        //    double dy = gEx.SpacingY * z / k;
        //    g.SpacingY = dy;
        //    g.SpacingX = dx;
        //    g.StartY = -0.5 * (g.Rows * dy);
        //    g.StartX = -0.5 * (g.Cols * dx);
        //    // output factor, phase calculation and modification
        //    Complex fac = -Complex.ImaginaryOne * k / z
        //        * Complex.Exp(Complex.ImaginaryOne * k * z);
        //    FieldCommons.AddQuadPhase(v: vEx, grid: g, c2x: kOver2D, c2y: kOver2D,
        //        scalFac: fac, loopMode: loopMode);
        //    FieldCommons.AddQuadPhase(v: vEy, grid: g, c2x: kOver2D, c2y: kOver2D,
        //        scalFac: fac, loopMode: loopMode);
        //}

        #endregion

    }
}
