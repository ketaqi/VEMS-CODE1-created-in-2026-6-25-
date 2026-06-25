using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents an identity optical component in 2D.
    /// This component does not alter the input field and is used as a placeholder or for interface consistency.
    /// </summary>
    public class Identity2D : IOpticalComponent
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
        /// Gets or sets the coordinate system of the thin component (input direction).
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

        // No additional properties for Identity2D.

        #endregion
        #region constructors

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Identity2D"/> class with the specified coordinate system.
        ///// </summary>
        ///// <param name="coordinate">The coordinate system representing the position and attitude of the component.</param>
        //internal Identity2D(CoordinateSystem? coordinate = null)
        //{
        //    Coordinate = coordinate ?? CoordinateSystem.Origin;
        //    //OutputCoordinate = Coordinate;
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="Identity2D"/> class at the origin.
        /// Sets the <see cref="Process"/> function to the identity operation.
        /// </summary>
        public Identity2D(string? label = null,
            CoordinateSystem? coordinate = null)
        {
            // optical component
            Label = label ?? GetType().FullName;
            Coordinate = coordinate ?? CoordinateSystem.Origin;
            Process = (v) => v;
            OutputCoordinate = Coordinate; // output coordinate is the same as the input for transmission models
        }

        #endregion
        #region methods

        // ...

        #endregion
    }

}
