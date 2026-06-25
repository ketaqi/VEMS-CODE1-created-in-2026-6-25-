using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{


    /// <summary>
    /// Implements the Fast Fourier Transform Beam Propagation Method (FFT-BPM) for 2D electromagnetic field propagation.
    /// Provides methods for propagating scalar fields through a layered medium using FFT-based techniques.
    /// </summary>
    public class FFTBPM2D
    {
        #region properties

        /// <summary>
        /// Gets or sets the z-invariant 2D medium in the XY plane for propagation.
        /// </summary>
        public Medium2D? MediumXY { get; set; } = null;

        /// <summary>
        /// Gets or sets the (x,y,z)-dependent 3D medium for propagation.
        /// </summary>
        public Medium3D? MediumXYZ { get; set; } = null;

        /// <summary>
        /// Gets or sets the optional 2D aperture filter applied during propagation.
        /// </summary>
        public Aperture2D? Filter { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of intermediate field values recorded during propagation.
        /// Each entry corresponds to the field after a propagation step.
        /// </summary>
        public List<MatrixZ>? FiledsInside { get; set; } = null;

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FFTBPM2D"/> class with default parameters.
        /// </summary>
        internal FFTBPM2D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FFTBPM2D"/> class with a specified 2D medium and optional aperture filter.
        /// </summary>
        /// <param name="mediumXY">The z-invariant 2D medium in the XY plane.</param>
        /// <param name="filter">Optional aperture filter to apply during propagation.</param>
        public FFTBPM2D(Medium2D mediumXY, Aperture2D? filter = null)
        {
            MediumXY = mediumXY;
            Filter = filter;
        }

        #endregion
        #region methods

        /// <summary>
        /// Propagates the scalar field <paramref name="v"/> by a single step of distance <paramref name="d"/>,
        /// applies the transmission function <paramref name="t"/>, and optionally applies an aperture <paramref name="a"/>.
        /// </summary>
        /// <param name="v">The scalar field to propagate and modulate (passed by reference).</param>
        /// <param name="d">The propagation distance for this step.</param>
        /// <param name="t">The transmission function for the current layer.</param>
        /// <param name="a">Optional aperture to modulate the field.</param>
        /// <param name="loopMode">Loop-computational mode option.</param>
        [Obsolete]
        private static void PropagateStep(ref SCField v, double d,
            Transmission2D t, Aperture2D? a,
            LoopMode loopMode)
        {
            v.Propagate(d: d,
                targetDomain: ModelingDomain.Spatial,
                loopMode: loopMode);
            t.ModulateOn(ref v, loopMode);
            a?.ModulateOn(ref v, loopMode);
        }

        /// <summary>
        /// Propagates the scalar field <paramref name="v"/> by a distance <paramref name="d"/>,
        /// then modulates the field in-place with the transmission function <paramref name="t"/>,
        /// and optionally with the aperture function <paramref name="a"/>.
        /// </summary>
        /// <param name="v">The scalar field to propagate and modulate (passed by reference).</param>
        /// <param name="d">The propagation distance for this step.</param>
        /// <param name="t">The transmission function as a complex matrix.</param>
        /// <param name="a">Optional aperture function as a complex matrix. If not null, applied after <paramref name="t"/>.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="v"/>.<c>UValues</c> is null.</exception>
        private static void PropagateStep(ref SCField v, double d,
            MatrixZ t, MatrixZ? a,
            LoopMode loopMode)
        {
            // Propagate the field by distance d
            v.Propagate(d: d, targetDomain: ModelingDomain.Spatial, loopMode: loopMode);
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            MatrixZ u = v.UValues;
            //VMath.MulZ(x: u, y: t, z: ref u);
            //if (a != null) { VMath.MulZ(x: u, y: a, z: ref u); }
            unsafe { Defaults.IVMF.Mul(n: u.Count, a: u.VPtr, b: t.VPtr, y: u.VPtr); }
            if (a != null)
            { unsafe { Defaults.IVMF.Mul(n: u.Count, a: u.VPtr, b: a.VPtr, y: u.VPtr); } }
        }


        /// <summary>
        /// Propagates an input field through the z-invariant 2D medium using the FFT-BPM algorithm.
        /// </summary>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="vIn">Input field values as a complex matrix.</param>
        /// <param name="gIn">Input grid information.</param>
        /// <param name="zStart">Starting z-coordinate of the propagation region.</param>
        /// <param name="zEnd">Ending z-coordinate of the propagation region.</param>
        /// <param name="nLayers">Number of propagation layers (steps).</param>
        /// <param name="n0">Reference refractive index (optional; if not set, uses the medium at (0,0)).</param>
        /// <param name="showProgress">If true, displays a progress bar during propagation.</param>
        /// <param name="loopMode">Loop-computational mode option.</param>
        /// <returns>
        /// The propagated field as a complex matrix at <paramref name="zEnd"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="MediumXY"/> is not set.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="zEnd"/> is not greater than <paramref name="zStart"/>.</exception>
        public MatrixZ Propagate(double wavelength,
            MatrixZ vIn, GridInfo2D gIn,
            double zStart, double zEnd, long nLayers,
            Complex n0 = default,
            bool showProgress = true,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // Validate input
            if (MediumXY == null) { throw new ArgumentNullException(nameof(MediumXY), "MediumX is not set!"); }
            if (zEnd <= zStart) { throw new ArgumentException("zEnd must be greater than zStart.", nameof(zEnd)); }

            // Use local variables for frequently accessed fields
            var filter = Filter;
            var medium = MediumXY;

            // Reference refractive index
            if (n0 == default) { n0 = medium.N(wavelength, 0.0, 0.0); }

            // Precompute layer thickness
            double dz = (zEnd - zStart) / nLayers;

            // Precompute transmission of a single layer
            Transmission2D t = TEA2D.Compute(
                layer: medium,
                wavelength: wavelength,
                thickness: dz,
                n0: n0,
                isPhaseOnly: false);
            MatrixZ txy = t.Sample(grid: gIn, loopMode: loopMode);

            // Precompute spatial filter (aperture) if provided
            MatrixZ? axy = null;
            if (Filter != null)
            { axy = Filter.Sample(grid: gIn, loopMode: loopMode); }

            // Construct input field
            var v = new SCField(
                wavelength: wavelength,
                material: new FuncMaterial(nReal: n0.Real, nImag: n0.Imaginary),
                uGrid: gIn,
                uValues: vIn,
                domain: ModelingDomain.Spatial);

            // First half layer propagation and modulation
            PropagateStep(ref v, 0.5 * dz, txy, axy, loopMode);

            // Main propagation loop
            if (nLayers > 1)
            {
                for (long i = 0, n = nLayers - 1; i < n; ++i)
                {
                    PropagateStep(ref v, dz, txy, axy, loopMode);
                    if (showProgress)
                    { Printer.ProgressBar(total: (int)n, idx: (int)i + 1, prompt: $"FFT-BPM Progress:"); }
                }
            }
            //if (showProgress) { Printer.WriteLine("\n"); }

            // Last half layer propagation
            v.Propagate(0.5 * dz, targetDomain: ModelingDomain.Spatial);

            // Return result, avoid unnecessary allocation if possible
            return v.UValues ?? MatrixZ.Empty;
        }

        #endregion

        #region derived

        /// <summary>
        /// Represents a circular tube (step-index fiber) medium for 2D FFT-BPM propagation.
        /// The medium consists of an inner circular core of specified diameter and material,
        /// surrounded by an outer cladding material. The refractive index is defined as
        /// <see cref="MatInner"/> for points within the core (radius ≤ 0.5 * <see cref="Diameter"/>),
        /// and <see cref="MatOuter"/> for points outside the core.
        /// Optionally, an aperture filter can be applied during propagation.
        /// </summary>
        public class CircTube : FFTBPM2D, IOpticalComponent
        {
            #region ---- IOpticalComponent ----

            /// <summary>
            /// Gets or sets the label associated with the optical component.
            /// </summary>
            public string? Label { get; set; } = null;

            /// <summary>
            /// Gets or sets the processing function for the optical component.
            /// This function defines how the component processes an <see cref="SCField"/> and returns the resulting <see cref="SCField"/>.
            /// </summary>
            public Func<SCField, SCField>? Process { get; set; } = null;

            /// <summary>
            /// Gets or sets the detection function for the optical component.
            /// This function defines how the component processes an <see cref="SCField"/> and returns the resulting <see cref="MatrixD"/>,
            /// typically representing a detected intensity or measurement on the field.
            /// </summary>
            public Func<SCField, Grid2DRealData>? Detect { get; set; } = null;

            /// <summary>
            /// Coodinate system of the thin component ( Input direction )
            /// </summary>
            public CoordinateSystem? Coordinate { get; set; } = null;

            /// <summary>
            /// Gets the output coordinate system of the optical component.
            /// If the <see cref="Coordinate"/> property is <c>null</c>, returns a new <see cref="CoordinateSystem"/>
            /// at the origin with zero rotation. Otherwise, returns the value of <see cref="Coordinate"/>.
            /// </summary>
            public CoordinateSystem? OutputCoordinate { get; set; } = null;

            #endregion

            #region properties

            /// <summary>
            /// Gets or sets the diameter of the inner circular region (tube core).
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// Gets or sets the length of the circular tube along the propagation (z) direction.
            /// This property defines the physical length of the step-index fiber or waveguide represented by this component.
            /// </summary>
            public double Length { get; set; }

            /// <summary>
            /// Gets or sets the material of the inner region (core) of the tube.
            /// </summary>
            public Material? MatInner { get; set; }

            /// <summary>
            /// Gets or sets the material of the outer region (cladding) of the tube.
            /// </summary>
            public Material? MatOuter { get; set; }

            #endregion
            #region constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="CircTube"/> class with default parameters.
            /// </summary>
            internal CircTube() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="CircTube"/> class with the specified diameter, length, inner and outer materials, and optional aperture filter.
            /// The 2D medium is constructed such that points with radius less than or equal to half the diameter use <paramref name="matInner"/>,
            /// and points outside use <paramref name="matOuter"/>.
            /// </summary>
            /// <param name="diameter">The diameter of the inner circular region (core).</param>
            /// <param name="length">The length of the circular tube along the propagation (z) direction.</param>
            /// <param name="matInner">The material of the inner region (core).</param>
            /// <param name="matOuter">The material of the outer region (cladding).</param>
            /// <param name="filter">Optional aperture filter to apply during propagation.</param>
            public CircTube(double diameter, double length,
                Material matInner, Material matOuter,
                Aperture2D? filter = null)
            {
                Diameter = diameter;
                Length = length;
                MatInner = matInner;
                MatOuter = matOuter;
                Filter = filter;
                // Create the 2D medium for the tube
                Complex n(double w, double x, double y)
                {
                    double rho2 = x * x + y * y;
                    double rho = Math.Sqrt(rho2);
                    if (rho <= 0.5 * Diameter)
                    { return matInner.N(w); }
                    else
                    { return matOuter.N(w); }
                }
                MediumXY = new Medium2D(n);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CircTube"/> class with the specified diameter, length, inner and outer materials, and optional aperture filter.
            /// The 2D medium is constructed such that points with radius less than or equal to half the diameter use <paramref name="matInner"/>,
            /// and points outside use <paramref name="matOuter"/>.
            /// </summary>
            /// <param name="diameter">The diameter of the inner circular region (core).</param>
            /// <param name="length">The length of the circular tube along the propagation (z) direction.</param>
            /// <param name="matInner">The material of the inner region (core).</param>
            /// <param name="matOuter">The material of the outer region (cladding).</param>
            /// <param name="filter">Optional aperture filter to apply during propagation.</param>
            /// <param name="label"></param>
            /// <param name="coordinate"></param>
            /// <param name="loopMode"></param>
            public CircTube(double diameter, double length, long nLayers,
                Material matInner, Material matOuter,
                Aperture2D? filter = null,
                string? label = null,
                CoordinateSystem? coordinate = null,
                LoopMode loopMode = Defaults.LoopOption)
            {
                Diameter = diameter;
                Length = length;
                MatInner = matInner;
                MatOuter = matOuter;
                Filter = filter;
                // Create the 2D medium for the tube
                Complex n(double w, double x, double y)
                {
                    double rho2 = x * x + y * y;
                    double rho = Math.Sqrt(rho2);
                    if (rho <= 0.5 * Diameter)
                    { return matInner.N(w); }
                    else
                    { return matOuter.N(w); }
                }
                MediumXY = new Medium2D(n);
                // optical component
                Label = label ?? GetType().FullName;
                Coordinate = coordinate ?? CoordinateSystem.Origin;
                Process = (v) =>
                {
                    SCField result = v;
                    Propagate(ref result, nLayers: nLayers, 
                        n0: matInner.N(0.0), showProgress: true, loopMode: loopMode);
                    return result;
                };
                OutputCoordinate = new (relativeCoordinate: Coordinate,
                    relativeLocation: new VecD3(0,0, Length), relativeRotation: VecD3.Zeros);
            }

            #endregion
            #region methods

            /// <summary>
            /// Propagates the input field <paramref name="v"/> through the circular tube (step-index fiber) medium
            /// using the FFT-BPM algorithm, for a specified number of layers.
            /// </summary>
            /// <typeparam name="T">A type derived from <see cref="SCField"/> representing the scalar field to propagate.</typeparam>
            /// <param name="v">The scalar field to propagate, passed by reference. The field is updated in-place.</param>
            /// <param name="nLayers">Number of propagation layers (steps) to use in the FFT-BPM algorithm.</param>
            /// <param name="n0">Reference refractive index (optional; if not set, uses the core material at the field's wavelength).</param>
            /// <param name="showProgress">If <c>true</c>, displays a progress bar during propagation.</param>
            /// <param name="loopMode">Loop-computational mode option for performance tuning.</param>
            public void Propagate<T>(ref T v,
                long nLayers,
                Complex n0 = default,
                bool showProgress = true,
                LoopMode loopMode = Defaults.LoopOption)
                where T : SCField
            {
                MatrixZ vIn = new (other: v.UValues, deepCopy: true);
                GridInfo2D g = new (other: v.UGrid);

                MatrixZ vOut = Propagate(v.Wavelength, vIn, g, 0.0, Length,
                    nLayers, n0, true, loopMode);

                v.UValues = vOut;
            }


            #endregion
        }

        #endregion
    }


    /// <summary>
    /// Placeholder for a 3D FFT-BPM implementation.
    /// </summary>
    internal class FFTBPMxyz
    {
        // ...
    }

}
