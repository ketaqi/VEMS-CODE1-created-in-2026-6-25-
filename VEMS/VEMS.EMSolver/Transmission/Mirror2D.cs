using VEMS.MathCore;

namespace VEMS.EMSolver
{

    ///<summary>
    /// Represents an ideal 2D mirror optical component.
    /// Inherits from <see cref="Identity2D"/> and does not alter the input field.
    /// Used as a placeholder or for interface consistency in 2D optical simulations.
    /// </summary>
    public class IdealMirror2D : Identity2D
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdealMirror2D"/> class with the specified coordinate system.
        /// If no coordinate system is provided, the origin is used as the default.
        /// </summary>
        /// <param name="coordinate">The coordinate system representing the position and attitude of the component.</param>
        public IdealMirror2D(string? label = null,
            CoordinateSystem? coordinate = null)
            : base(label, coordinate)
        {
            // No additional initialization required for IdealMirror2D.
        }
    }


    /// <summary>  
    /// Represents a 2D mirror optical component.  
    /// This class provides properties and methods to define the position, attitude, and behavior of the mirror  
    /// in a 2D coordinate system, including processing scalar fields based on loop modes.  
    /// </summary>  
    [Obsolete]
    public class Mirror2D : IOpticalComponent
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
        public Func<SCField, SCField>? Process { get; set; } 

        /// <summary>
        /// Gets or sets the detection function for the optical component.
        /// This function defines how the component processes an <see cref="SCField"/> and returns the resulting <see cref="MatrixD"/>,
        /// typically representing a detected intensity or measurement on the field.
        /// </summary>
        public Func<SCField, Grid2DRealData>? Detect { get; set; } 

        /// <summary>
        /// Coodinate system of the thin component ( Input direction )
        /// </summary>
        public CoordinateSystem? Coordinate { get; set; }

        /// <summary>
        /// Gets the output coordinate system of the optical component.
        /// If the <see cref="Coordinate"/> property is <c>null</c>, returns a new <see cref="CoordinateSystem"/>
        /// at the origin with zero rotation. Otherwise, returns the value of <see cref="Coordinate"/>.
        /// </summary>
        public CoordinateSystem? OutputCoordinate { get; set; }
        //{
        //    get
        //    {
        //        // If the Coordinate property is null, create a new CoordinateSystem with the grid's position.
        //        if (Coordinate is null)
        //        { return new CoordinateSystem(location: VecD3.Zeros, rotation: VecD3.Zeros); }
        //        else // output coordinate is the same as the input for transmission models
        //        { return Coordinate; }
        //    }
        //}

        #endregion

        #region properties  

        /// <summary>  
        /// Gets or sets the shift in the X direction relative to the previous component. 
        /// </summary>  
        public double ShiftX { get; set; }

        /// <summary>  
        /// Gets or sets the shift in the Y direction relative to the previous component.
        /// </summary>  
        public double ShiftY { get; set; }

        #endregion
        #region constructors  
        /// <summary>  
        /// Initializes a new instance of the <see cref="Mirror2D"/> class with the specified coordinate system.  
        /// If no coordinate system is provided, the origin is used as the default.  
        /// </summary>  
        /// <param name="coordinate">The coordinate system representing the position and attitude of the component.</param>  
        public Mirror2D(CoordinateSystem? coordinate = null)
        {
            Coordinate = coordinate ?? CoordinateSystem.Origin;
            //OutputCoordinate = Coordinate;
            ShiftX = Coordinate.RelativeLocation.X;
            ShiftY = Coordinate.RelativeLocation.Y;
        }
        #endregion
        #region methods  

        ///// <summary>  
        ///// Processes the given scalar field based on the loop mode.  
        ///// </summary>  
        ///// <typeparam name="T">The type of the scalar field to process.</typeparam>  
        ///// <param name="v">The scalar field to process.</param>  
        ///// <param name="loopMode">The loop mode to use during processing. Defaults to <see cref="Defaults.LoopOption"/>.</param>  
        //public void Process<T>(ref T v, LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
        //{

        //}

        #endregion
    }

}