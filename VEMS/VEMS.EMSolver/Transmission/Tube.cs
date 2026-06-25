
namespace VEMS.EMSolver
{

    ///// <summary>
    ///// An optical element that has a refractive index change in the x and y directions and cannot be considered as a thin element
    ///// </summary>
    //public class Tube : IOpticalComponent
    //{
    //    #region properties  


    //    public Func<SCField, SCField>? Process { get; set; }

    //    /// <summary>  
    //    /// Record the position and attitude of the component.  
    //    /// </summary>  
    //    public CoordinateSystem? Coordinate { get; set; }

    //    /// <summary>  
    //    /// Output coordinate system after the tube, which is offset by TubeLength along the Z-axis from the Coordinate property.  
    //    /// </summary>  
    //    public CoordinateSystem? OutputCoordinate
    //    {
    //        get
    //        {
    //            // If the Coordinate property is null, create a new CoordinateSystem with a position offset by TubeLength along the Z-axis.  
    //            if (Coordinate is null)
    //                return new CoordinateSystem(new VecD3(0, 0, TubeLength), new VecD3(0, 0, 0));
    //            else
    //            {
    //                // Otherwise, create a copy of the existing CoordinateSystem and adjust its Z-axis position by adding TubeLength.  
    //                CoordinateSystem temp = new CoordinateSystem(Coordinate);
    //                temp.RelativeLocation.Z = temp.RelativeLocation.Z + TubeLength;
    //                return temp;
    //            }
    //        }
    //    }

    //    /// <summary>  
    //    /// Wavelength of the field.  
    //    /// </summary>  
    //    public double WaveLength { get; set; }

    //    /// <summary>  
    //    /// Shift in the X direction.  
    //    /// </summary>  
    //    public double ShiftX { get; set; }

    //    /// <summary>  
    //    /// Shift in the Y direction.  
    //    /// </summary>  
    //    public double ShiftY { get; set; }

    //    /// <summary>  
    //    /// Length of the tube.  
    //    /// </summary>  
    //    public double TubeLength { get; set; }

    //    /// <summary>  
    //    /// Number of layers to calculate.  
    //    /// </summary>  
    //    public long NLayers { get; set; }

    //    /// <summary>  
    //    /// Beam propagation method used for processing.  
    //    /// </summary>  
    //    public BeamPropagationMethod BPM { get; set; }

    //    private Func<double, double, Complex>? nFunc;

    //    /// <summary>  
    //    /// Function to calculate refractive index based on shifted coordinates.  
    //    /// </summary>  
    //    public Func<double, double, Complex> NFunc
    //    {
    //        get { return (x, y) => nFunc(x - ShiftX, y - ShiftY); }
    //        set { nFunc = value; }
    //    }
    //    #endregion
    //    #region constructors  
    //    /// <summary>  
    //    /// Constructor for the Tube class.  
    //    /// </summary>  
    //    /// <param name="wavelength">Wavelength of the field.</param>  
    //    /// <param name="tubeLength">Length of the tube.</param>  
    //    /// <param name="nLayers">Number of layers to calculate.</param>  
    //    /// <param name="coordinate">Coordinate system of the tube.</param>  
    //    /// <param name="bpm">Beam propagation method.</param>  
    //    public Tube(double wavelength, double tubeLength, long nLayers, BeamPropagationMethod bpm = BeamPropagationMethod.FTBPM, CoordinateSystem? coordinate = null)
    //    {
    //        WaveLength = wavelength;
    //        TubeLength = tubeLength;
    //        NLayers = nLayers;
    //        Coordinate = coordinate ?? CoordinateSystem.Origin;
    //        ShiftX = Coordinate.RelativeLocation.X;
    //        ShiftY = Coordinate.RelativeLocation.Y;
    //        BPM = bpm;
    //    }
    //    #endregion
    //    #region methods  

    //    ///// <summary>  
    //    ///// Processes the scalar field using the specified beam propagation method.  
    //    ///// </summary>  
    //    ///// <typeparam name="T">Type of the scalar field.</typeparam>  
    //    ///// <param name="v">Reference to the scalar field to process.</param>  
    //    ///// <param name="loopMode">Loop mode for processing.</param>  
    //    //public void Process<T>(ref T v, LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
    //    //{
    //    //    switch (BPM)
    //    //    {
    //    //        case BeamPropagationMethod.FTBPM:
    //    //            {
    //    //                ThinLayer tubeLayer = new((w, x, y, p) => NFunc(x, y), 0.0);
    //    //                FTBPM ftBPM = new(tubeLayer, new List<double>(), TubeLength, NLayers, false, loopMode);
    //    //                ScalarField temp = ftBPM.Propagate(v);
    //    //                v.Field = temp.Field;
    //    //            }
    //    //            break;
    //    //        case BeamPropagationMethod.RKBPM:
    //    //            throw new NotImplementedException("Beam propagation method not implemented");
    //    //    }
    //    //}

    //    #endregion
    //    #region derived sub-classes  
    //    /// <summary>  
    //    /// Represents a circular tube with a specified diameter.  
    //    /// </summary>  
    //    public class Circular : Tube
    //    {
    //        #region properties  
    //        /// <summary>  
    //        /// Diameter of the circular tube.  
    //        /// </summary>  
    //        public double Diameter { get; set; }
    //        #endregion

    //        #region constructors  
    //        /// <summary>  
    //        /// Constructor for the Circular tube class.  
    //        /// </summary>  
    //        /// <param name="wavelength">Wavelength of the field.</param>  
    //        /// <param name="diameter">Diameter of the circular tube.</param>  
    //        /// <param name="core">Core material of the tube.</param>  
    //        /// <param name="clad">Cladding material of the tube.</param>  
    //        /// <param name="tubeLength">Length of the tube.</param>  
    //        /// <param name="nLayers">Number of layers to calculate.</param>  
    //        /// <param name="bpm">Beam propagation method.</param>  
    //        /// <param name="coordinate">Coordinate system of the tube.</param>  
    //        public Circular(double wavelength, double diameter, Material core, Material clad, double tubeLength, long nLayers,
    //            BeamPropagationMethod bpm = BeamPropagationMethod.FTBPM, CoordinateSystem? coordinate = null)
    //        : base(wavelength, tubeLength, nLayers, bpm, coordinate)
    //        {
    //            Diameter = diameter;
    //            NFunc = (x, y) => NTube(x, y, Complex.Sqrt(core.Epsilon(wavelength)), Complex.Sqrt(clad.Epsilon(wavelength)), diameter);
    //        }
    //        #endregion

    //        /// <summary>  
    //        /// Calculates the refractive index based on the position within the circular tube.  
    //        /// </summary>  
    //        /// <param name="x">X-coordinate.</param>  
    //        /// <param name="y">Y-coordinate.</param>  
    //        /// <param name="nCore">Refractive index of the core material.</param>  
    //        /// <param name="nClad">Refractive index of the cladding material.</param>  
    //        /// <param name="d">Diameter of the tube.</param>  
    //        /// <returns>Refractive index at the specified position.</returns>  
    //        private Complex NTube(double x, double y, Complex nCore, Complex nClad, double d)
    //        {
    //            var rho2 = x * x + y * y;
    //            var rho = Math.Sqrt(rho2);
    //            if (rho <= 0.5 * d) { return nCore; }
    //            else { return nClad; }
    //        }
    //    }
    //    #endregion
    //}

    /// <summary>
    /// Change the algorithm for handling beam propagation
    /// </summary>
    public enum BeamPropagationMethod
    {
        /// <summary>
        /// Fast Fourier Beam Propagation Method (FTBPM)
        /// </summary>
        FTBPM,
        /// <summary>
        /// Runge-Kutta Beam Propagation Method (RKBPM)
        /// </summary>
        RKBPM
    }
}
