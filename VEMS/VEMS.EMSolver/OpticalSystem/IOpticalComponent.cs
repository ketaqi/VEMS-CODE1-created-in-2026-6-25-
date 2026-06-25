using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents an optical component within the system.
    /// </summary>
    public interface IOpticalComponent
    {
        /// <summary>
        /// Gets or sets the label associated with the current object.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the processing function for the optical component.
        /// This function defines how the component processes an <see cref="SCField"/> and returns the resulting <see cref="SCField"/>.
        /// </summary>
        Func<SCField, SCField>? Process { get; set; }

        /// <summary>
        /// Gets or sets the detection function for the optical component.
        /// This function defines how the component detects or measures an <see cref="SCField"/> and returns the resulting <see cref="MatrixD"/>.
        /// </summary>
        Func<SCField, Grid2DRealData>? Detect { get; set; }

        //Func<SCField, List<SCField>>? Generate { get; set; }

        /// <summary>
        /// Gets or sets the coordinate system associated with the optical component.
        /// </summary>
        CoordinateSystem? Coordinate { get; set; }

        /// <summary>
        /// Gets the output coordinate system of the optical component.
        /// </summary>
        CoordinateSystem? OutputCoordinate { get; set; }

        ///// <summary>
        ///// Processes the given scalar field based on the specified loop mode.
        ///// </summary>
        ///// <typeparam name="T">The type of the scalar field to process.</typeparam>
        ///// <param name="v">The scalar field to process, passed by reference.</param>
        ///// <param name="loopMode">The loop mode to use during processing. Defaults to <see cref="Defaults.LoopOption"/>.</param>
        //void Process<T>(ref T v, LoopMode loopMode = Defaults.LoopOption) where T : ScalarField;

        


    }
}
