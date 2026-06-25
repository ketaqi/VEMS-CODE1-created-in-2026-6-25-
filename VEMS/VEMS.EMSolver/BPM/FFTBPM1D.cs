using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Implements the 1D Fourier-transform-based beam propagation method (FT-BPM)
    /// for simulating the propagation of scalar fields through a z-invariant medium.
    /// Supports optional spatial filtering and custom reference refractive index.
    /// </summary>
    public class FFTBPM1D
    {
        #region properties

        /// <summary>
        /// Gets or sets the z-invariant 1D medium through which the field propagates.
        /// </summary>
        public Medium1D? MediumX { get; set; } = null;

        /// <summary>
        /// Gets or sets the (x, z)-dependent medium through which the field propagates.
        /// </summary>
        public Medium2D? MediumXZ { get; set; } = null;

        /// <summary>
        /// Gets or sets the optional spatial filter (aperture) applied after each layer.
        /// </summary>
        public Aperture1D? Filter { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of intermediate field values recorded during propagation.
        /// Each entry corresponds to the field after a propagation step.
        /// </summary>
        public List<VectorZ>? FieldsInside { get; set; } = null;

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FFTBPM1D"/> class with default values.
        /// </summary>
        internal FFTBPM1D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FFTBPM1D"/> class with the specified z-invariant 1D medium and optional spatial filter.
        /// </summary>
        /// <param name="mediumX">The z-invariant 1D medium through which the field propagates.</param>
        /// <param name="filter">The optional spatial filter (aperture) applied after each layer. If null, no filter is applied.</param>
        public FFTBPM1D(Medium1D mediumX, Aperture1D? filter = null)
        {
            MediumX = mediumX;
            Filter = filter;
        }


        internal FFTBPM1D(Medium2D mediumXZ, Aperture1D? filter = null)
        {
            MediumXZ = mediumXZ;
            Filter = filter;
        }

        #endregion
        #region methods

        #region ---- single step ----

        /// <summary>
        /// Propagates a 1D scalar field through a single step, applies the transmission function,
        /// and optionally applies a spatial filter (aperture), all in-place.
        /// </summary>
        /// <param name="v">The 1D scalar field to propagate and modulate.</param>
        /// <param name="d">The propagation distance of current step.</param>
        /// <param name="t">The transmission function to modulate the field.</param>
        /// <param name="a">The spatial filter (aperture) to optionally apply after modulation.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning.</param>
        [Obsolete]
        private static void PropagateStep(ref SCField1D v, double d,
            Transmission1D t, Aperture1D? a,
            LoopMode loopMode)
        {
            v.Propagate(d: d,
                targetDomain: ModelingDomain.Spatial,
                loopMode: loopMode);
            t.ModulateOn(ref v, loopMode);
            a?.ModulateOn(ref v, loopMode);
        }

        /// <summary>
        /// Propagates a 1D scalar field through a single step, applies the transmission function,
        /// and optionally applies a spatial filter (aperture), all in-place.
        /// </summary>
        /// <param name="v">The 1D scalar field to propagate and modulate.</param>
        /// <param name="d">The propagation distance of the current step.</param>
        /// <param name="t">The transmission function sampled on the grid, to modulate the field.</param>
        /// <param name="a">The spatial filter (aperture) sampled on the grid, to optionally apply after modulation. If null, no filter is applied.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="v"/>.<c>UValues</c> is null.</exception>
        private static void PropagateStep(ref SCField1D v, double d,
            VectorZ t, VectorZ? a,
            LoopMode loopMode)
        {
            v.Propagate(d: d, targetDomain: ModelingDomain.Spatial);
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            VectorZ u = v.UValues;
            //VMath.MulZ(x: u, y: t, z: ref u);
            //if (a != null) { VMath.MulZ(x: u, y: a, z: ref u); }
            unsafe { Defaults.IVMF.Mul(n: u.Count, a: u.VPtr, b: t.VPtr, y: u.VPtr); }
            if (a != null)
            { unsafe { Defaults.IVMF.Mul(n: u.Count, a: u.VPtr, b: a.VPtr, y: u.VPtr); } }
        }

        #endregion
        #region ---- z-invariant ----

        /// <summary>
        /// Propagates a 1D complex field through the specified medium using the FT-BPM algorithm.
        /// Optionally applies a spatial filter after each layer and allows for a custom reference refractive index.
        /// </summary>
        /// <param name="wavelength">The wavelength in vacuum.</param>
        /// <param name="vIn">The input field values as a complex vector.</param>
        /// <param name="gIn">The input field's 1D grid information.</param>
        /// <param name="zStart">The starting position along the Z-axis.</param>
        /// <param name="zEnd">The end position along the Z-axis.</param>
        /// <param name="nLayers">The number of layer divisions for the propagation.</param>
        /// <param name="n0">The reference refractive index (optional; if not specified, uses the medium's value at x=0).</param>
        /// <param name="showProgress">If true, displays a progress bar during propagation.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning.</param>
        /// <returns>The propagated field as a complex vector on the same grid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="MediumX"/> is not set.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="zEnd"/> is not greater than <paramref name="zStart"/>.</exception>
        public VectorZ Propagate(double wavelength,
            VectorZ vIn, GridInfo1D gIn,
            double zStart, double zEnd, long nLayers,
            Complex n0 = default,
            bool showProgress = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // Validate input
            if (MediumX == null) { throw new ArgumentNullException(nameof(MediumX), "MediumX is not set!"); }
            if (zEnd <= zStart) { throw new ArgumentException("zEnd must be greater than zStart.", nameof(zEnd)); }

            // Use local variables for frequently accessed fields
            var filter = Filter;
            var medium = MediumX;

            // Reference refractive index
            if (n0 == default) { n0 = medium.N(wavelength, 0.0); }

            // Precompute layer thickness
            double dz = (zEnd - zStart) / nLayers;

            // Precompute transmission of a single layer
            Transmission1D t = TEA1D.Compute(
                layer: medium,
                wavelength: wavelength,
                thickness: dz,
                n0: n0,
                isPhaseOnly: false);
            VectorZ tx = t.Sample(grid: gIn, loopMode: loopMode);

            // Precompute spatial filter (aperture) if provided
            VectorZ? ax = null;
            if (Filter != null)
            { ax = Filter.Sample(grid: gIn, loopMode: loopMode); }

            // Construct input field
            SCField1D v = new (
                wavelength: wavelength,
                material: new FuncMaterial(nReal: n0.Real, nImag: n0.Imaginary),
                uGrid: gIn,
                uValues: vIn,
                domain: ModelingDomain.Spatial);

            // First half layer propagation and modulation
            PropagateStep(ref v, 0.5 * dz, tx, ax, loopMode);

            // Main propagation loop
            if (nLayers > 1)
            {
                for (long i = 0, n = nLayers - 1; i < n; ++i)
                {
                    PropagateStep(ref v, dz, tx, ax, loopMode);
                    if (showProgress)
                    { Printer.ProgressBar(total: (int)n, idx: (int)i + 1, prompt: $"FFT-BPM Progress:"); }
                }
            }

            // Last half layer propagation
            v.Propagate(0.5 * dz, targetDomain: ModelingDomain.Spatial);

            // Return result, avoid unnecessary allocation if possible
            return v.UValues ?? VectorZ.Empty;
        }

        /// <summary>
        /// Propagates a 1D complex field through the specified medium using the FT-BPM algorithm.
        /// Records the intermediate field values after each propagation step in <paramref name="fieldsInside"/>.
        /// Optionally applies a spatial filter after each layer and allows for a custom reference refractive index.
        /// </summary>
        /// <param name="wavelength">The wavelength in vacuum.</param>
        /// <param name="vIn">The input field values as a complex vector.</param>
        /// <param name="gIn">The input field's 1D grid information.</param>
        /// <param name="zStart">The starting position along the Z-axis.</param>
        /// <param name="zEnd">The end position along the Z-axis.</param>
        /// <param name="nLayers">The number of layer divisions for the propagation.</param>
        /// <param name="fieldsInside">
        /// Output list to store the intermediate field values after each propagation step.
        /// Each entry corresponds to the field after a propagation step.
        /// </param>
        /// <param name="n0">The reference refractive index (optional; if not specified, uses the medium's value at x=0).</param>
        /// <param name="showProgress">If true, displays a progress bar during propagation.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning.</param>
        /// <returns>The propagated field as a complex vector on the same grid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="MediumX"/> is not set.</exception>
        public VectorZ Propagate(double wavelength,
            VectorZ vIn, GridInfo1D gIn,
            double zStart, double zEnd, long nLayers,
            out List<VectorZ> fieldsInside,
            Complex n0 = default,
            bool showProgress = false,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // Validate input
            if (MediumX == null) { throw new ArgumentNullException(nameof(MediumX), "MediumX is not set!"); }
            if (zEnd <= zStart) { throw new ArgumentException("zEnd must be greater than zStart.", nameof(zEnd)); }

            // Initialize fieldsInside if not already set
            fieldsInside = new List<VectorZ>(capacity: (int)nLayers);

            // Use local variables for frequently accessed fields
            var filter = Filter;
            var medium = MediumX;

            // Reference refractive index
            if (n0 == default) { n0 = medium.N(wavelength, 0.0); }

            // Precompute layer thickness
            double dz = (zEnd - zStart) / nLayers;

            // Precompute transmission of a single layer
            Transmission1D t = TEA1D.Compute(
                layer: medium,
                wavelength: wavelength,
                thickness: dz,
                n0: n0,
                isPhaseOnly: false);
            VectorZ tx = t.Sample(grid: gIn, loopMode: loopMode);

            // Precompute spatial filter (aperture) if provided
            VectorZ? ax = null;
            if (Filter != null)
            { ax = Filter.Sample(grid: gIn, loopMode: loopMode); }

            // Construct input field
            SCField1D v = new (
                wavelength: wavelength,
                material: new FuncMaterial(nReal: n0.Real, nImag: n0.Imaginary),
                uGrid: gIn,
                uValues: vIn,
                domain: ModelingDomain.Spatial);

            // First half layer propagation and modulation
            PropagateStep(ref v, 0.5 * dz, tx, ax, loopMode);
            if (v.UValues is not null)
            { fieldsInside.Add(new(other: v.UValues, deepCopy: true)); }

            // Main propagation loop
            if (nLayers > 1)
            {
                for (long i = 0, n = nLayers - 1; i < n; ++i)
                {
                    PropagateStep(ref v, dz, tx, ax, loopMode);
                    if (showProgress)
                    { Printer.ProgressBar(total: (int)n, idx: (int)i + 1, prompt: $"FFT-BPM Progress:"); }
                    if (v.UValues is not null)
                    { fieldsInside.Add(new(other: v.UValues, deepCopy: true)); }
                }
            }

            // Last half layer propagation
            v.Propagate(0.5 * dz, targetDomain: ModelingDomain.Spatial);

            // Return result, avoid unnecessary allocation if possible
            return v.UValues ?? VectorZ.Empty;
        }

        #endregion
        #region ---- z-dependent ----

        // ...

        #endregion

        #endregion
    }


    internal class FFTBPMxz
    {
        // ...
    }



}
