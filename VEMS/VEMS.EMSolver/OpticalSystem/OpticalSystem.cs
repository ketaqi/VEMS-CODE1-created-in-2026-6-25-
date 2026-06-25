using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents an optical system consisting of multiple optical components.
    /// </summary>
    public class OpticalSystem
    {
        #region properties

        /// <summary>
        /// Gets or sets the list of optical components in the system.
        /// </summary>
        public List<IOpticalComponent> Components { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of detection results for each optical component in the system.
        /// Each entry corresponds to the output of a detection function applied to the scalar field after processing by a component.
        /// </summary>
        public List<Grid2DRealData> DetResults { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary mapping optical components to the scalar fields behind them.
        /// </summary>
        public Dictionary<IOpticalComponent, ScalarField> FieldsBehindComponents { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether to save the fields behind components.
        /// </summary>
        public bool SaveFields { get; set; } = false;
        #endregion
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OpticalSystem"/> class.
        /// </summary>
        public OpticalSystem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpticalSystem"/> class with specified components and save fields option.
        /// </summary>
        /// <param name="opticalComponents">The list of optical components.</param>
        /// <param name="saveFields">Indicates whether to save the fields behind components.</param>
        public OpticalSystem(List<IOpticalComponent> opticalComponents, bool saveFields = false)
        {
            Components = opticalComponents;
            SaveFields = saveFields;
        }

        #endregion
        #region methods

        /// <summary>
        /// Evaluates the optical system by propagating the input scalar field <paramref name="v"/> through each optical component in sequence.
        /// For each component, the method optionally logs progress, propagates the field according to the component's coordinate system,
        /// applies the component's process function if defined, and performs detection if a detect function is available.
        /// Detection results are stored in <see cref="DetResults"/>.
        /// </summary>
        /// <param name="v">Reference to the input <see cref="SCField"/> to be propagated and processed through the system.</param>
        /// <param name="showLogging">If <c>true</c>, logs progress and details for each component during evaluation. Default is <c>true</c>.</param>
        /// <param name="loopMode">Specifies the loop-computational mode for propagation. Default is <see cref="Defaults.LoopOption"/>.</param>
        public void Evaluate(ref SCField v,
            bool showLogging = true,
            LoopMode loopMode = Defaults.LoopOption)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                var component = Components[i];

                // start 
                if (showLogging)
                { Printer.WriteLine($"--- Processing component {i + 1}/{Components.Count}: {component.Label} starts ..."); }

                // coordinate
                var coord = component.Coordinate;
                if (coord == null) { Printer.WriteLine($"=> No coordinate system defined for the current component."); }
                else
                {
                    if (showLogging) { Printer.WriteLine($"=> Coordinate system: location @X: {coord.Location.X} [m], Y: {coord.Location.Y} [m], Z: {coord.Location.Z} [m]."); }
                    //Printer.WriteLine($"---- Coordinate system: orientation @{coord.} 
                    double z = coord.RelativeLocation.Z;
                    if (showLogging) { Printer.WriteLine($"=> Relative distance (z): {z} [m] ----"); }
                    if (z != 0.0)
                    {
                        v.Propagate(d: z, loopMode: loopMode);
                        if (showLogging) { Printer.WriteLine($"=> Propagated (SPW method) to the current component."); }
                    }

                }

                // process
                if (component.Process == null) { Printer.WriteLine($"=> No process function defined for the current component."); }
                else
                {
                    v = component.Process!(v);
                    if (showLogging) { Printer.WriteLine($"=> Process function performed for the current component."); }
                }

                // detect
                if (component.Detect != null)
                {
                    DetResults.Add(component.Detect(v));
                    if (showLogging) { Printer.WriteLine($"=> Detection function performed for thecurrent component."); }
                }

                // final
                if (showLogging)
                { Printer.WriteLine($"--- Processing component {i + 1}/{Components.Count}: {component.Label} finished."); }
            }
        }


        /// <summary>
        /// Processes the scalar field through the optical system components.
        /// </summary>
        /// <typeparam name="T">The type of the scalar field.</typeparam>
        /// <param name="v">The scalar field to process.</param>
        /// <param name="loopMode">The loop mode for processing. Defaults to <see cref="Defaults.LoopOption"/>.</param>
        public void Process<T>(ref T v, LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
        {
            if (Components.Count == 0)
            {
                return; // nothing to process
            }
            foreach (var component in Components)
            {
                // handle possible distance from previous component
                if (component.Coordinate != null && component.Coordinate.RelativeLocation.Z != 0.0)
                {
                    v.Propagate(component.Coordinate.RelativeLocation.Z, loopMode: loopMode);
                }
                // handle possible rotation
                v.SwitchToXDomain();
                // modulate the field with the component
                //component.Process(ref v, loopMode);
                //v = Process(v);
                if (SaveFields)
                {
                    // save the field behind the component
                    FieldsBehindComponents[component] = new ScalarField(v);
                }
            }
        }

        /// <summary>
        /// Adds a new optical component to the system.
        /// </summary>
        /// <param name="component">The optical component to add.</param>
        public void AddComponent(IOpticalComponent component)
        {
            Components.Add(component);
        }
        #endregion
    }
}
