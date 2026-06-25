using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// 1D scalar field, invariant along y
    /// e.g. a V(x) component
    /// </summary>
    public class ScalarField1D : FieldBase
    {
        #region properties

        /// <summary>
        /// sign factor according to direction
        /// </summary>
        public SignFactor Direction { get; set; }

        /// <summary>
        /// domain in which EM field is given/calculated
        /// </summary>
        public ModelingDomain Domain { get; set; }

        // trial ...
        //public Func<double, Complex> FindFieldValue { get; set; }

        /// <summary>
        /// sampling grid information
        /// in the current domain
        /// </summary>
        public GridInfo1D Grid { get; set; }

        ///// <summary>
        ///// transverse spatial frequencies
        ///// </summary>
        //public VectorD Kx { get => K0 * Nx; }

        /// <summary>
        /// normalized transverse spatial frequencies
        /// Nx = Kx / K0
        /// </summary>
        public VectorD? Nx { get; set; } // => Kx / K0;

        /// <summary>
        /// normalized eigenvalues Nz = Kz/K0 
        /// </summary>
        public VectorZ? Nz { get; set; }

        ///// <summary>
        ///// Kz = K0 * Nz
        ///// </summary>
        //public VectorZ Kz { get => K0 * Nz; }

        ///// <summary>
        ///// eigenvalues gamma
        ///// </summary>
        //public VectorZ Gamma => Complex.ImaginaryOne * Kz;

        /// <summary>
        /// complex field component e.g. a E-field component
        /// </summary>
        public VectorZ? Field { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public ScalarField1D() { }

        /// <summary>
        /// constructs a ScalarField1D
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> relative permittivity @wavelength </param>
        /// <param name="mu"> relative permeability @wavelength </param>
        /// <param name="grid"> 1D sampling grid information </param>
        /// <param name="fieldValues"> vector that contains the field values </param>
        /// <param name="domain"> modeling domain in which the field is specified </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public ScalarField1D(double wavelength, Complex epsilon, Complex mu,
            GridInfo1D grid, 
            VectorZ? fieldValues = null,
            ModelingDomain domain = ModelingDomain.Spatial,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : base(wavelength, epsilon, mu)
        {
            if (fieldValues != null && grid.Count != fieldValues.Count)
                throw new ArgumentException();
            // gets input parameters
            Direction = direction;
            Domain = domain;
            Grid = grid;
            Field = fieldValues ?? null;
            if (initializeEigenInfo) { ComputeEigenInfo(); }
        }

        /// <summary>
        /// constructs a ScalarField1D
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index </param>
        /// <param name="grid"> 1D sampling grid information </param>
        /// <param name="fieldValues"> vector that contains the field values </param>
        /// <param name="domain"> modeling domain in which the field is specified </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public ScalarField1D(double wavelength, Complex n,
            GridInfo1D grid, 
            VectorZ? fieldValues = null,
            ModelingDomain domain = ModelingDomain.Spatial,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true) 
            : this(wavelength: wavelength, epsilon: n*n, mu: 1.0,
                  grid: grid, fieldValues: fieldValues,
                  domain: domain, direction: direction, 
                  initializeEigenInfo: initializeEigenInfo)
        { }

        /// <summary>
        /// constructs a ScalarField1D by copying from another
        /// </summary>
        /// <param name="other"> another ScalarField1D as the source </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public ScalarField1D(ScalarField1D other, 
            bool initializeEigenInfo = true) 
            : base(other.Wavelength, other.Epsilon)
        {
            // copy simple parameters from another field
            Direction = other.Direction;
            Domain = other.Domain;
            // copy field data
            Grid = new GridInfo1D(other.Grid);
            Field = (other.Field==null)? null : new VectorZ(other.Field, true);
            // initialize eigen information
            if (initializeEigenInfo) { ComputeEigenInfo(); }
        }

        #endregion
        #region methods

        #region ----- eigen-info -----

        /// <summary>
        /// comnputes the eigen information
        /// </summary>
        public void ComputeEigenInfo(LoopMode loopMode = Defaults.LoopOption)
        {
            if (Grid == null) { return; }
            // defines k-domain grid
            GridInfo1D kGrid = new(Grid);
            if (Domain == ModelingDomain.Spatial) { kGrid.GetConjugated(isForward: true); }
            // computes nx
            Nx = kGrid.GetCoordinates() / K0;
            // using UniformLayer for EigenInfo calculation
            UniformLayer freeSpace = new(epsilon: Epsilon, mu: Mu, thickness: 0.0);
            Nz = freeSpace.ComputeNz(wavelength: Wavelength, nx: Nx, 
                loopMode: loopMode); 
        }

        #endregion
        #region ----- domain-transform -----

        /// <summary>
        /// switch to the spatial frequency (K) domain
        /// </summary>
        public void SwitchToKDomain()
        {
            if (Domain == ModelingDomain.SpatialFrequency) { return; }
            // perform forward transform
            if (Field == null) { return; }
            //Transform.FFT(Field, Grid, FFTOption.Forward);
            VectorZ x = Field;
            GridInfo1D g = Grid;
            Transform.FFT1D(x: ref x, grid: ref g,
                direction: FFTOptions.Direction.Forward);
            // change the domain flag
            Domain = ModelingDomain.SpatialFrequency;
        }

        /// <summary>
        /// switch to the spatial (X) domain
        /// </summary>
        public void SwitchToXDomain()
        {
            if (Domain == ModelingDomain.Spatial) { return; }
            // perform backward transform
            if (Field == null) { return; }
            //Transform.FFT(Field, Grid, FFTOption.Backward);
            VectorZ x = Field;
            GridInfo1D g = Grid;
            Transform.FFT1D(x: ref x, grid: ref g,
                direction: FFTOptions.Direction.Backward);
            // change the domain flag
            Domain = ModelingDomain.Spatial;
        }

        #endregion
        #region ----- size-manipulation -----

        /// <summary>
        /// zero-valued centeral padding of the field data
        /// </summary>
        /// <param name="targetCount"> targer number of elements after padding </param>
        public void Padding(long targetCount)
        {
            if(Field == null) { return; }
            Field = Field.Padding(targetCount);
            Grid = new(n: targetCount,
                start: Grid.Start - (targetCount - Grid.Count)/2 * Grid.Spacing,
                spacing: Grid.Spacing);
        }

        /// <summary>
        /// central truncation of the field data
        /// </summary>
        /// <param name="targetCount"> target number of elements after truncation </param>
        public void Truncate(long targetCount)
        {
            if(Field == null) { return; }
            Field = Field.Truncate(targetCount);
            Grid = new(n: targetCount,
                start: Grid.Start + (Grid.Count - targetCount) / 2 * Grid.Spacing,
                spacing: Grid.Spacing);
        }

        #endregion
        #region ----- propagation -----

        /// <summary>
        /// propagation in the spatial frequency domain
        /// </summary>
        /// <param name="d"> propagation distance along z-axis </param>
        /// <param name="targetDomain"></param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="sizeFactor"> padding or truncation factor in x-domain </param>
        /// <param name="kSizeFactor"> (central) filtering factor in k-domain </param>
        /// <param name="kEdgeRatio"> smooth edge factor for k-domain truncation, w.r.t. truncation width </param>
        public void Propagate(double d, 
            ModelingDomain targetDomain = ModelingDomain.SpatialFrequency,
            LoopMode loopMode = Defaults.LoopOption,
            double sizeFactor = 1.0,
            double kSizeFactor = 1.0, double kEdgeRatio = 0.2)
        {
            if(Field == null) { return; }
            // [possible] input field padding
            if (sizeFactor > 1.0)
            {
                SwitchToXDomain(); // makes sure to pad in the x-domain
                long targetCount = (long)(Field.Count * sizeFactor);
                if ((targetCount - Field.Count) % 2 != 0) { targetCount++; }
                Padding(targetCount);
            }
            // [possible] input domain handling
            SwitchToKDomain();
            // [possible] truncation in k-domain
            if(kSizeFactor < 1.0)
            {
                // filter width in k-domain
                double wKx = Grid.Range * kSizeFactor;
                Samp1DRealFunc t = new(Function1D.CosEdgeRectangle,
                        new List<double> { wKx, kEdgeRatio * wKx, 0.0, 1.0 });
                Field *= t.Sample(Grid);
            }
            // SPW kernel
            if(Nz == null) { throw new ArgumentNullException("Nz"); }
            VectorZ values = Field;
            SPW.Propagate1D(wavelength: Wavelength,
                v: ref values, // to be modified 
                nz: Nz, z: d, loopMode: loopMode);
            // [possible] output field truncation
            if (sizeFactor < 1.0)
            {
                SwitchToXDomain(); // makes sure to truncate in the x-domain
                long targetCount = (long)(Field.Count * sizeFactor);
                if ((Field.Count - targetCount) % 2 != 0) { targetCount--; }
                Truncate(targetCount);
            }
            // [possible] output domain handling
            if(targetDomain == ModelingDomain.Spatial) { SwitchToXDomain(); }
            else { SwitchToKDomain(); }
        }

        #endregion

        #endregion
        #region derived sub-classes

        /// <summary>
        /// 1D scalar Gaussian field at its waist 
        /// </summary>
        public class Gaussian : ScalarField1D
        {
            #region properties

            /// <summary>
            /// radius of the waist
            /// </summary>
            public double WaistRadius { get; set; }

            /// <summary>
            /// lateral shift of the Gaussian field 
            /// </summary>
            public double Shift { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain
            /// i.e. the linear phase in spatial domain
            /// </summary>
            public double Kx0 { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// default constructor
            /// </summary>
            public Gaussian() { }

            /// <summary>
            /// constructs a scalar Gaussian field with specific parameters
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="n"> complex refractive index @wavelength </param>
            /// <param name="waistRadius"> waist radius </param>
            /// <param name="grid"> 1D sampling grid information </param>
            /// <param name="shift"> lateral shift </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase </param>
            public Gaussian(double wavelength, Complex n,
                double waistRadius, GridInfo1D grid,
                double shift = 0.0, double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0) 
                : base(wavelength: wavelength, n: n, grid: grid, 
                      domain: domain, direction: direction,
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                WaistRadius = waistRadius;
                Shift = shift;
                Scaling = scale;
                Kx0 = kx0;

                // fills field values
                Samp1DCplxFunc sf = new(f: (x) => Function1D.Gaussian(x, [WaistRadius]));
                Field = sf.Sample(grid: Grid, loopMode: loopMode);
                //// handles possible linear phase
                //if (Kx0 != 0.0) { FieldCommons.AddLinearPhase(v: Field, grid: Grid, c1: Kx0); }
            }

            #endregion
        }

        /// <summary>
        /// 1D scalar truncated plane wave field
        /// </summary>
        public class PlaneWave : ScalarField1D
        {
            #region properties

            /// <summary>
            /// truncation diameter of the field
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// absolute edge width (half inside diameter, half outside)
            /// </summary>
            public double EdgeWidth { get; set; }

            /// <summary>
            /// lateral shift of the truncated plane wave
            /// </summary>
            public double Shift { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain
            /// i.e. the linear phase in spatial domain
            /// </summary>
            public double Kx0 { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// default constructor
            /// </summary>
            public PlaneWave() { }

            /// <summary>
            /// constructs a truncated plane wave with specific parameters
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="n"> complex refractive index @wavelength </param>
            /// <param name="diameter"> diameter of the truncation </param>
            /// <param name="grid"> 1D uniform sampling grid </param>
            /// <param name="edge"> absolute edge width (half inside, half outside) </param>
            /// <param name="shift"> lateral shift of the field </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase </param>
            public PlaneWave(double wavelength, Complex n, 
                double diameter, GridInfo1D grid,
                double edge = 0.0, double shift = 0.0, double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0) 
                : base(wavelength: wavelength, n: n, grid: grid,
                      domain: domain, direction: direction, 
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                Diameter = diameter;
                EdgeWidth = edge;
                Shift = shift;
                Scaling = scale;
                Kx0 = kx0;

                // fills field values
                Aperture1D.Rectangular a = new(Diameter, EdgeWidth, Shift, Scaling);
                Field = a.Sample(grid: Grid, loopMode: loopMode);
                //// handles possible linear phase
                //if (Kx0 != 0.0) { FieldCommons.AddLinearPhase(v: Field, grid: Grid, c1: Kx0); }
            }

            #endregion
        }

        /// <summary>
        /// 1D scalar truncated cylindrical wave field
        /// </summary>
        public class CylindricalWave : ScalarField1D
        {
            #region properties

            /// <summary>
            /// distance from the point source
            /// </summary>
            public double SourceDistance { get; set; }

            /// <summary>
            /// truncation diameter of the field
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// absolute edge width (half inside diameter, half outside)
            /// </summary>
            public double EdgeWidth { get; set; }

            /// <summary>
            /// lateral shift of the truncated plane wave
            /// </summary>
            public double Shift { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain
            /// i.e. the linear phase in spatial domain
            /// </summary>
            public double Kx0 { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// default constructor
            /// </summary>
            public CylindricalWave() { }

            /// <summary>
            /// constructs a cylindrical wave that is truncated on a plane
            /// at a certain distance from the point source
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="n"> complex refractive index @wavelength </param>
            /// <param name="z"> distance from the point source </param>
            /// <param name="alpha"> half-opening angle </param>
            /// <param name="grid"> 1D sampling grid information </param>
            /// <param name="edge"> absolute edge width (half inside, half outside) </param>
            /// <param name="shift"> lateral shift of the field on the plane </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase </param>
            public CylindricalWave(double wavelength, Complex n,
                double z, double alpha, GridInfo1D grid,
                double edge = 0.0, double shift = 0.0, double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0) 
                : base(wavelength: wavelength, n: n, grid: grid, 
                      domain: domain, direction: direction,
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                SourceDistance = Math.Abs(z);
                Diameter = 2.0 * Math.Tan(alpha) * SourceDistance;
                EdgeWidth = edge;
                Shift = shift;
                Scaling = scale;
                Kx0 = kx0;

                // fills field values
                Aperture1D.Rectangular a = new(Diameter, EdgeWidth, Shift, Scaling);
                Field = a.Sample(grid: Grid, loopMode: loopMode);
                VectorZ t = Field;
                FieldCommons.AddCylindPhase(x: ref t, grid: Grid,
                    z: SourceDistance, c: K0 * RefractiveIndex.Real,
                    loopMode: loopMode);
                //// handles possible linear phase
                //if (kx0 != 0.0) { FieldCommons.AddLinearPhase(v: Field, grid: Grid, c1: kx0); }
            }

            #endregion
        }

        /// <summary>
        /// 1D scalar field with Gaussian amplitude
        /// and cylindrical wavefront phase
        /// </summary>
        public class GaussCylindWave : ScalarField1D
        {
            #region properties

            /// <summary>
            /// distance from the point source
            /// </summary>
            public double SourceDistance { get; set; }

            /// <summary>
            /// truncation diameter of the field
            /// </summary>
            public double Diameter { get; set; }

            /// <summary>
            /// lateral shift of the truncated plane wave
            /// </summary>
            public double Shift { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain
            /// i.e. the linear phase in spatial domain
            /// </summary>
            public double Kx0 { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// default constructor
            /// </summary>
            public GaussCylindWave() { }

            /// <summary>
            /// constructs a cylindrical wave that is truncated on a plane
            /// at a certain distance from the point source
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="n"> complex refractive index @wavelength </param>
            /// <param name="z"> distance from the point source </param>
            /// <param name="alpha"> half-opening angle </param>
            /// <param name="grid"> 1D sampling grid information </param>
            /// <param name="shift"> lateral shift of the field on the plane </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase </param>
            public GaussCylindWave(double wavelength, Complex n,
                double z, double alpha, GridInfo1D grid,
                double shift = 0.0, double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0)
                : base(wavelength: wavelength, n: n, grid: grid,
                      domain: domain, direction: direction,
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                SourceDistance = Math.Abs(z);
                Diameter = 2.0 * Math.Tan(alpha) * SourceDistance;
                Shift = shift;
                Scaling = scale;
                Kx0 = kx0;

                // fills field values
                Aperture1D.Gaussian a = new(0.5 * Diameter, Shift, Scaling);
                Field = a.Sample(grid: Grid, loopMode: loopMode);
                VectorZ t = Field;
                FieldCommons.AddCylindPhase(x: ref t, grid: Grid, 
                    z: SourceDistance, c: K0 * RefractiveIndex.Real, 
                    loopMode: loopMode);
                //// handles possible linear phase
                //if (kx0 != 0.0) { FieldCommons.AddLinearPhase(v: Field, grid: Grid, c1: kx0); }
            }

            #endregion
        }

        #endregion
    }
}
