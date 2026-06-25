using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// collection of typical free-space propagation algorithms
    /// </summary>
    public class FreeSpace : IOpticalComponent
    {
        #region static methods collection

        #region ---- SPW 1D [k-domain] ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// with given nz = kz/k0 information
        /// </summary>
        /// <param name="v"> scalar field (in/out) in k-domain </param>
        /// <param name="distance"> propagation distance </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public static void SPW1D<T>(ref T v, double distance,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField1D
        {
            // exception handling
            if (v.Domain != ModelingDomain.SpatialFrequency)
            { throw new ArgumentException($"Input not given in spatial frequency domain"); }
            if (v.UValues == null)
            { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.Nz == null)
            { throw new ArgumentNullException(nameof(v.Nz)); }

            // propagation kernel
            VectorZ values = v.UValues;
            SPW.Propagate1D(wavelength: v.Wavelength,
                v: ref values, // to be modified ...
                nz: v.Nz, z: distance, loopMode: loopMode);
        }

        // EMField ...

        #endregion
        #region ---- SPW 2D [k-domain] ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Spectrum-of-Plane-Wave (SPW) method
        /// </summary>
        /// <param name="v"> scalar field (in/out) in k-domain </param>
        /// <param name="distance"> propagation distance </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public static void SPW2D<T>(ref T v, double distance,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField
        {
            // exception handling
            if (v.Domain != ModelingDomain.SpatialFrequency)
            { throw new ArgumentException($"Input not given in spatial frequency domain"); }
            if (v.UValues == null)
            { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.Nz == null)
            { throw new ArgumentNullException(nameof(v.Nz)); }

            // propagation kernel
            MatrixZ values = v.UValues;
            SPW.Propagate2D(wavelength: v.Wavelength,
                v: ref values, // to be modified ...
                nz: v.Nz, z: distance, loopMode: loopMode);
        }

        #endregion

        #region ---- Fresnel 1D [x-domain] ----

        /// <summary>
        /// Propagates a 1D scalar field to a parallel plane at a given distance
        /// using the Fresnel diffraction integral method with paraxial approximation.
        /// The propagation is performed in-place on the residual field part U.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the scalar field, which must inherit from <see cref="SCField1D"/>.
        /// </typeparam>
        /// <param name="v">
        /// The scalar field to propagate (in/out). The propagation modifies the field in-place.
        /// </param>
        /// <param name="distance">
        /// The propagation distance along the z-axis.
        /// </param>
        /// <param name="targetDomain">
        /// The target domain of the result after propagation. Defaults to <see cref="ModelingDomain.Spatial"/>.
        /// If set to <see cref="ModelingDomain.SpatialFrequency"/>, the field is transformed to the spatial-frequency domain after propagation.
        /// </param>
        /// <param name="loopMode">
        /// Loop-computational mode option. Controls parallelization and computational strategy. Defaults to <see cref="Defaults.LoopOption"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="v"/>'s <c>UValues</c> or <c>UGrid</c> is <c>null</c>.
        /// </exception>
        public static void Fresnel1D<T>(ref T v, double distance,
            ModelingDomain targetDomain = ModelingDomain.Spatial,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField1D
        {
            // Fast null checks and early exit
            if (v.UValues is null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid is null) { throw new ArgumentNullException(nameof(v.UGrid)); }

            // input domain handling
            v.SwitchToXDomain();
            VectorZ values = v.UValues;
            GridInfo1D grid = v.UGrid;

            // Fresnel propagation kernel
            Fresnel.Propagate1D(
                wavelength: v.Wavelength,
                nReal: v.Material.NReal(v.Wavelength),
                v: ref values, // modified in-place
                g: ref grid, // modified in-place
                z: distance,
                loopMode: loopMode);

            // Only switch domain if needed
            if (targetDomain == ModelingDomain.SpatialFrequency)
            { v.SwitchToKDomain(); }
        }

        #endregion
        #region ---- Fresnel 2D [x-domain] ----

        /// <summary>
        /// Propagates a 2D scalar field to a parallel plane at a given distance
        /// using the Fresnel diffraction integral method with paraxial approximation.
        /// The propagation is performed in-place on the residual field part U.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the scalar field, which must inherit from <see cref="SCField"/>.
        /// </typeparam>
        /// <param name="v">
        /// The scalar field to propagate (in/out). The propagation modifies the field in-place.
        /// </param>
        /// <param name="distance">
        /// The propagation distance along the z-axis.
        /// </param>
        /// <param name="targetDomain">
        /// The target domain of the result after propagation. Defaults to <see cref="ModelingDomain.Spatial"/>.
        /// If set to <see cref="ModelingDomain.SpatialFrequency"/>, the field is transformed to the spatial-frequency domain after propagation.
        /// </param>
        /// <param name="loopMode">
        /// Loop-computational mode option. Controls parallelization and computational strategy. Defaults to <see cref="Defaults.LoopOption"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="v"/>'s <c>UValues</c> or <c>UGrid</c> is <c>null</c>.
        /// </exception>
        public static void Fresnel2D<T>(ref T v, double distance,
            ModelingDomain targetDomain = ModelingDomain.Spatial,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField
        {
            // Fast null checks and early exit
            if (v.UValues is null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid is null) { throw new ArgumentNullException(nameof(v.UGrid)); }

            // input domain handling
            v.SwitchToXDomain();
            MatrixZ values = v.UValues;
            GridInfo2D grid = v.UGrid;

            // Fresnel propagation kernel
            Fresnel.Propagate2D(
                wavelength: v.Wavelength,
                nReal: v.Material.NReal(v.Wavelength),
                v: ref values, // modified in-place
                g: ref grid, // modified in-place
                z: distance,
                loopMode: loopMode);

            // Only switch domain if needed
            if (targetDomain == ModelingDomain.SpatialFrequency)
            { v.SwitchToKDomain(); }
        }

        // EMField ...

        #endregion

        #region ---- Fraunhofer 1D [x-domain] ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Fraunhofer diffraction integral method
        /// with paraxial and far-field approximation
        /// </summary>
        /// <param name="v"> scalar field (in/out) </param>
        /// <param name="distance"> propagation distance </param>
        /// <param name="targetDomain"> target domain of the result </param>
        /// <param name="loopMode"> loop-computational mode option </param>
        public static void Fraunhofer1D<T>(ref T v, double distance,
            ModelingDomain targetDomain = ModelingDomain.Spatial,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField1D
        {
            // exception handling
            if (v.UValues == null)
            { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null)
            { throw new ArgumentNullException(nameof(v.UGrid)); }

            // input domain handling
            v.SwitchToXDomain();
            VectorZ values = v.UValues;
            GridInfo1D grid = v.UGrid;
            Fraunhofer.Propagate1D(wavelength: v.Wavelength, 
                nReal: v.Material.NReal(v.Wavelength), 
                v: ref values, // to be modified ...
                g: ref grid, // to be modified ...
                z: distance,
                loopMode: loopMode);

            // output domain handling
            if (targetDomain == ModelingDomain.SpatialFrequency)
            { v.SwitchToKDomain(); }
        }

        // EMField ...

        #endregion
        #region ---- Fraunhofer 2D [x-domain] ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using the Fraunhofer diffraction integral method
        /// with paraxial and far-field approximation
        /// </summary>
        /// <param name="v"> scalar field (in/out) </param>
        /// <param name="distance"> propagation distance </param>
        /// <param name="targetDomain"> target domain of the result </param>
        /// <param name="loopMode"> loop-computational mode option </param>
        public static void Fraunhofer2D<T>(ref T v, double distance,
            ModelingDomain targetDomain = ModelingDomain.Spatial,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField
        {
            if (v.UValues == null)
            { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null)
            { throw new ArgumentNullException(nameof(v.UGrid)); }

            // input domain handling
            v.SwitchToXDomain();
            MatrixZ values = v.UValues;
            GridInfo2D grid = v.UGrid;
            Fraunhofer.Propagate2D(wavelength: v.Wavelength, 
                nReal: v.Material.NReal(v.Wavelength),
                v: ref values, // to be modified ...
                g: ref grid, // to be modified ...
                z: distance,
                loopMode: loopMode);

            // output domain handling
            if (targetDomain == ModelingDomain.SpatialFrequency)
            { v.SwitchToKDomain(); }
        }

        // EMField ...

        #endregion

        #region ---- Rayleigh-Sommerfeld 1D ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using Rayleigh-Sommerfeld diffraction integral
        /// [only for real-valued refractivce index]
        /// </summary>
        /// <param name="v"> scalar field (in/out) in x-domain </param>
        /// <param name="distance"> propagation distance </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public static void RayleighSommerfeld1D<T>(ref T v, double distance,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField1D
        {
            // exception handling
            if (v.Domain != ModelingDomain.Spatial)
            { throw new ArgumentException($"Input not given in spatial domain"); }
            if (v.UValues == null)
            { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null)
            { throw new ArgumentNullException(nameof(v.UGrid)); }

            // propagation kernel
            VectorZ values = v.UValues;
            GridInfo1D grid = v.UGrid;
            values = RayleighSommerfeld.Propagate1D(wavelength: v.Wavelength,
                nReal: v.Material.NReal(v.Wavelength), 
                vIn: values, gIn: grid,
                gTarget: grid, zs: new(count: grid.Count, initVal: distance),
                loopMode: loopMode);
            // update field values
            v.UValues = values;
        }

        // EMField ...

        #endregion
        #region ---- Rayleigh-Sommerfeld 2D ----

        /// <summary>
        /// propagates to a parallel plane at given distance
        /// using Rayleigh-Sommerfeld method
        /// </summary>
        /// <param name="v"> scalar field (in/out) in k-domai9n </param>
        /// <param name="distance"> propagation distance </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        public static void RayleighSommerfeld2D<T>(ref T v, double distance,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField
        {
            // exception handling
            if (v.Domain != ModelingDomain.Spatial)
            { throw new ArgumentException($"Input not given in spatial domain"); }
            if (v.UValues == null)
            { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null)
            { throw new ArgumentNullException(nameof(v.UGrid)); }

            // propagation kernel
            MatrixZ values = v.UValues;
            GridInfo2D grid = v.UGrid;
            values = RayleighSommerfeld.Propagate2D(wavelength: v.Wavelength,
                nReal: v.Material.NReal(v.Wavelength),
                vIn: values, gIn: grid,
                gTarget: grid, zs: new(grid.Rows, grid.Cols, distance),
                loopMode: loopMode);
            // update field values
            v.UValues = values;
        }

        // EMField ...


        #endregion

        #endregion

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
        /// Gets or sets the material associated with this free-space optical component.
        /// The material defines the refractive index and other optical properties used for propagation.
        /// </summary>
        public Material Mat { get; set; }

        /// <summary>
        /// Gets or sets the length of the free-space optical component.
        /// This property defines the propagation distance (in meters) through the free-space region
        /// and is used as the default propagation distance for all propagation methods in this component.
        /// </summary>
        public double Length { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeSpace"/> class with the specified material and length.
        /// </summary>
        /// <param name="mat">The material associated with this free-space optical component.</param>
        /// <param name="length">The propagation distance (in meters) through the free-space region.</param>
        /// <param name="label"></param>
        /// <param name="coordinate"></param>
        /// <param name="loopMode"></param>
        public FreeSpace(Material mat, double length,
            string? label = null,
            CoordinateSystem? coordinate = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            Mat = mat;
            Length = length;
            // optical component
            Label = label ?? GetType().FullName;
            Coordinate = coordinate ?? CoordinateSystem.Origin;
            Process = (v) =>
            {
                SCField result = v;
                Propagate(ref result,
                    dx: null, dy: null,
                    targetDomain: ModelingDomain.SpatialFrequency,
                    loopMode: loopMode);
                return result;
            };
            OutputCoordinate = new(relativeCoordinate: Coordinate,
                relativeLocation: new VecD3(0,0, Length), relativeRotation: VecD3.Zeros);
        }

        #endregion
        #region methods

        /// <summary>
        /// Propagates the given scalar field through the free-space optical component.
        /// This method updates the field's material to match the component's material if necessary,
        /// recomputes eigen information, and then propagates the field over the component's length.
        /// Optional parameters allow for control over grid spacing, target domain, computational mode,
        /// and domain size factors for both spatial and frequency domains.
        /// </summary>
        /// <typeparam name="T">The type of the scalar field, which must inherit from <see cref="SCField"/>.</typeparam>
        /// <param name="v">The scalar field to propagate (in/out). The propagation modifies the field in-place.</param>
        /// <param name="dx">Optional: Lateral shift in the x-direction after propagation. If <c>null</c>, it is computed automatically.</param>
        /// <param name="dy">Optional: Lateral shift in the y-direction after propagation. If <c>null</c>, it is computed automatically.</param>
        /// <param name="targetDomain">
        /// The target modeling domain after propagation (spatial or spatial-frequency).
        /// Defaults to <see cref="ModelingDomain.SpatialFrequency"/>.
        /// </param>
        /// <param name="loopMode">
        /// The loop-computational mode option for propagation. Controls parallelization and computational strategy.
        /// Defaults to <see cref="Defaults.LoopOption"/>.
        /// </param>
        /// <param name="xSizeFactor">Padding or truncation factor for field size in the x-domain. Defaults to 1.0 (no change).</param>
        /// <param name="kSizeFactor">Central filtering factor in the k-domain. Defaults to 1.0 (no filtering).</param>
        /// <param name="kEdgeRatio">Smooth edge factor for k-domain truncation, relative to truncation width. Defaults to 0.2.</param>
        public void Propagate<T>(ref T v,
            double? dx = null, double? dy = null,
            ModelingDomain targetDomain = ModelingDomain.SpatialFrequency,
            LoopMode loopMode = Defaults.LoopOption,
            double xSizeFactor = 1.0,
            double kSizeFactor = 1.0, double kEdgeRatio = 0.2)
            where T : SCField
        {
            //if (v.Material != Mat)
            //{
            //    Printer.Warning($"Different material encountered");
            //    v.Material = Mat; // update material
            //    v.ComputeEigenInfo();
            //}
            v.Propagate(d: Length,
                dx: dx, dy: dy,
                targetDomain: targetDomain,
                loopMode: loopMode,
                xSizeFactor: xSizeFactor,
                kSizeFactor: kSizeFactor,
                kEdgeRatio: kEdgeRatio);
        }

        #endregion

    }

}
