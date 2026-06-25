using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// 2D scalar field, given on x-y plane
    /// e.g. a V(x,y) component
    /// </summary>
    public class ScalarField : FieldBase
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

        /// <summary>
        /// sampling grid information
        /// in the current domain
        /// </summary>
        public GridInfo2D Grid { get; set; }

        ///// <summary>
        ///// transverse spatial frequencies Kx = K0 * Nx
        ///// </summary>
        //public VectorD Kx { get => K0 * Nx; }

        ///// <summary>
        ///// transverse spatial frequencies Ky = K0 * Ny
        ///// </summary>
        //public VectorD Ky { get => K0 * Ny; }

        /// <summary>
        /// normalized transverse spatial frequencies
        /// Nx = Kx / K0
        /// </summary>
        public VectorD? Nx { get; set; }

        /// <summary>
        /// normalized transverse spatial frequencies
        /// Ny = Ky / K0
        /// </summary>
        public VectorD? Ny { get; set; }

        /// <summary>
        /// normalized eigenvalues Nz = Kz/K0 
        /// </summary>
        public MatrixZ? Nz { get; set; }

        ///// <summary>
        ///// Kz = K0 * Nz
        ///// </summary>
        //public MatrixZ Kz => K0 * Nz;

        ///// <summary>
        ///// eigenvalues gamma
        ///// </summary>
        //public MatrixZ Gamma => Complex.ImaginaryOne * K0 * Nz;

        /// <summary>
        /// complex field component
        /// </summary>
        public MatrixZ? Field { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public ScalarField() { }

        /// <summary>
        /// constructs a scalar field
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> relative permittivity @wavelength </param>
        /// <param name="mu"> relative permeability @wavelength </param>
        /// <param name="grid"> 2D sampling grid information </param>
        /// <param name="fieldValues"> matrix that contains the field values </param>
        /// <param name="domain"> modeling domain in which the field is specified </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public ScalarField(double wavelength, Complex epsilon, Complex mu,
            GridInfo2D grid, 
            MatrixZ? fieldValues = null,
            ModelingDomain domain = ModelingDomain.Spatial,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : base(wavelength, epsilon, mu)
        {
            if (fieldValues != null &&
                (grid.Rows != fieldValues.Rows || grid.Cols != fieldValues.Cols))
                throw new ArgumentException();
            // gets input parameters
            Direction = direction;
            Domain = domain;
            Grid = grid;
            Field = fieldValues;
            if (initializeEigenInfo) { ComputeEigenInfo(); }
        }

        /// <summary>
        /// constructs a scalar field
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="n"> complex refractive index </param>
        /// <param name="grid"> 2D sampling grid information </param>
        /// <param name="fieldValues"> matrix that contains the field values </param>
        /// <param name="domain"> modeling domain in which the field is specified </param>
        /// <param name="direction"> sign factor for the direction </param>
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public ScalarField(double wavelength, Complex n,
            GridInfo2D grid, 
            MatrixZ? fieldValues = null,
            ModelingDomain domain = ModelingDomain.Spatial,
            SignFactor direction = SignFactor.Positive,
            bool initializeEigenInfo = true)
            : this(wavelength: wavelength, epsilon: n*n, mu: 1.0,
                  grid: grid, fieldValues: fieldValues,
                  domain: domain, direction: direction,
                  initializeEigenInfo: initializeEigenInfo)
        { }

        /// <summary>
        /// constructs a ScalarField by copying from another
        /// </summary>
        /// <param name="other"> another ScalarField as the source </param> 
        /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
        public ScalarField(ScalarField other,
            bool initializeEigenInfo = true) 
            : base(other.Wavelength, other.Epsilon)
        {
            // copy simple parameters from another field
            Direction = other.Direction;
            Domain = other.Domain;
            // copy (deeply) other parameters
            Grid = new GridInfo2D(other.Grid);
            Field = (other.Field == null)? null : new MatrixZ(other.Field, true);
            // initialize eigen-info
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
            GridInfo2D kGrid = new(Grid);
            if (Domain == ModelingDomain.Spatial) { kGrid.GetConjugated(isForward: true); }
            Ny = kGrid.GetCoordinatesY() / K0;
            Nx = kGrid.GetCoordinatesX() / K0;
            UniformLayer freeSpace = new(epsilon: Epsilon, mu: Mu, thickness: 0.0);
            Nz = freeSpace.ComputeNz(wavelength: Wavelength, nx: Nx, ny: Ny,
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
            MatrixZ x = Field;
            GridInfo2D g = Grid;
            Transform.FFT2D(x: ref x, grid: ref g,
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
            MatrixZ x = Field;
            GridInfo2D g = Grid;
            Transform.FFT2D(x: ref x, grid: ref g,
                direction: FFTOptions.Direction.Backward);
            // change the domain flag
            Domain = ModelingDomain.Spatial;
        }

        #endregion
        #region ----- size-manipulation -----

        /// <summary>
        /// zero-valued centeral padding of the field data
        /// </summary>
        /// <param name="targetRows"> target number of rows after padding </param>
        /// <param name="targetCols"> target number of columns after padding </param>
        public void Padding(long targetRows, long targetCols)
        {
            if (Field == null) { return; }
            Field = Field.Padding(targetRows, targetCols);
            Grid = new(rows: targetRows, cols: targetCols,
                startY: Grid.StartY - (targetRows - Grid.Rows)/2 * Grid.SpacingY,
                startX: Grid.StartX - (targetCols - Grid.Cols)/2 * Grid.SpacingX,
                spacingY: Grid.SpacingY, spacingX: Grid.SpacingX);
        }

        /// <summary>
        /// central truncation of the field data
        /// </summary>
        /// <param name="targetRows"> target number of rows after truncation </param>
        /// <param name="targetCols"> target number of columns after truncation </param>
        public void Truncate(long targetRows, long targetCols)
        {
            if (Field == null) { return; }
            Field = Field.Truncate(targetRows, targetCols);
            Grid = new(rows: targetRows, cols: targetCols,
                startY: Grid.StartY + (Grid.Rows - targetRows) / 2 * Grid.SpacingY,
                startX: Grid.StartX + (Grid.Cols - targetCols) / 2 * Grid.SpacingX,
                spacingY: Grid.SpacingY, spacingX: Grid.SpacingX);
        }

        #endregion
        #region ----- propagation -----

        /// <summary>
        /// propagation in the spatial frequency domain
        /// </summary>
        /// <param name="d"> propagation distance along z-axis </param>
        /// <param name="targetDomain"></param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <param name="sizeFactor"> padding or truncation factor for field size in the x-domain</param>
        /// <param name="kSizeFactor"> (central) filtering factor in k-domain </param>
        /// <param name="kEdgeRatio"> smooth edge factor for k-domain truncation, w.r.t. truncation width </param>
        public void Propagate(double d,
            ModelingDomain targetDomain = ModelingDomain.SpatialFrequency,
            LoopMode loopMode = Defaults.LoopOption,
            double sizeFactor = 1.0,
            double kSizeFactor = 1.0, double kEdgeRatio = 0.2)
        {
            if (Field == null) { return; }
            // [possible] input field padding
            if (sizeFactor > 1.0)
            {
                SwitchToXDomain(); // makes sure to pad in the x-domain
                long targetRows = (long)(Field.Rows * sizeFactor);
                long targetCols = (long)(Field.Cols * sizeFactor);
                if ((targetRows - Field.Rows) % 2 != 0) { targetRows++; }
                if ((targetCols - Field.Cols) % 2 != 0) { targetCols++; }
                Padding(targetRows, targetCols);
            }
            // [possible] input domain handling
            SwitchToKDomain();
            // [possible] truncation in k-domain
            if (kSizeFactor < 1.0)
            {
                // filter width in k-domain: circular aperture filtering
                double wK = Math.Min(Grid.RangeX, Grid.RangeY) * kSizeFactor;
                //Aperture2D.Ellptical t = new(diameterX: wK, diameterY: wK,
                //    edgeWidth: kEdgeRatio * wK);
                Aperture2D.Ellptical t = new(
                    diameterX: wK,
                    diameterY: wK,
                    edgeWidth: kEdgeRatio * wK,
                    shiftX: 0.0,
                    shiftY: 0.0,
                    scaling: 1.0);
                Field *= t.Sample(Grid);
            }
            // SPW kernel
            if (Nz == null) { throw new ArgumentNullException("Nz"); }
            MatrixZ values = Field;
            SPW.Propagate2D(wavelength: Wavelength,
                v: ref values, // to be modified ... 
                nz: Nz, z: d, loopMode: loopMode);
            // [possible] output field truncation
            if(sizeFactor < 1.0)
            {
                SwitchToXDomain(); // makes sure to truncate in the x-domain
                long targetRows = (long)(Field.Rows * sizeFactor);
                long targetCols = (long)(Field.Cols * sizeFactor);
                if((Field.Rows - targetRows) % 2 != 0) { targetRows--; }
                if((Field.Cols - targetCols) % 2 != 0) { targetCols--; }
                Truncate(targetRows, targetCols);
            }
            // [possible] output domain handling
            if (targetDomain == ModelingDomain.Spatial) { SwitchToXDomain(); }
            else { SwitchToKDomain(); }
        }

        #endregion

        #endregion
        #region derived sub-classes

        /// <summary>
        /// 2D scalar Gaussian field at its waist
        /// </summary>
        public class Gaussian : ScalarField
        {
            #region properties

            /// <summary>
            /// radius of the waist along x
            /// </summary>
            public double WaistRadiusX { get; set; }

            /// <summary>
            /// radius of the waist along y
            /// </summary>
            public double WaistRadiusY { get; set; }

            /// <summary>
            /// lateral shift along x
            /// </summary>
            public double ShiftX { get; set; }

            /// <summary>
            /// lateral shift along y
            /// </summary>
            public double ShiftY { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain along x
            /// i.e. the linear phase in spatial domain
            /// </summary>
            public double Kx0 { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain along y
            /// i.e. the linear phase in spatial domain
            /// </summary>
            public double Ky0 { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// default constructor
            /// </summary>
            public Gaussian() { }

            /// <summary>
            /// constructs a Gaussian field at its waist 
            /// with specific parameters
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="n"> complex refractive index @wavelength </param>
            /// <param name="waistRadiusX"> waist radius along x direction </param>
            /// <param name="waistRadiusY"> waist radius along y direction </param>
            /// <param name="grid"> 2D uniform sampling grid </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase along x direction </param>
            /// <param name="ky0"> spatial frequency that defines the linear phase along y direction </param>
            public Gaussian(double wavelength, Complex n,
                double waistRadiusX, double waistRadiusY, GridInfo2D grid,
                double shiftX = 0.0, double shiftY = 0.0, double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0, double ky0 = 0.0)
                : base(wavelength: wavelength, n: n, grid: grid,
                      domain: domain, direction: direction,
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                WaistRadiusX = waistRadiusX;
                WaistRadiusY = waistRadiusY;
                ShiftX = shiftX;
                ShiftY = shiftY;
                Scaling = scale;
                Kx0 = kx0;
                Ky0 = ky0;

                // fills the field values
                Aperture2D.Gaussian a = new(diameterX: 2.0 * waistRadiusX, diameterY: 2.0 * waistRadiusY,
                    shiftX: ShiftX, shiftY: ShiftY, scaling: Scaling);
                //ApertureXY.Gaussian a = new(WaistRadiusX, WaistRadiusY, ShiftX, ShiftY, Scaling);
                Field = a.Sample(grid: Grid, loopMode: loopMode);
                // handles possible linear phase
                MatrixZ t = Field;
                if(Kx0 != 0.0 || Ky0 != 0.0) { FieldCommons.AddLinearPhase(x: ref t, grid: Grid, c1x: Kx0, c1y: Ky0, loopMode: loopMode); }
            }

            #endregion
        }

        /// <summary>
        /// 2D scalar truncated plane wave field
        /// </summary>
        public class PlaneWave: ScalarField
        {
            #region properties

            /// <summary>
            /// truncation diameter of the field along x direction
            /// </summary>
            public double DiameterX {  get; set; }

            /// <summary>
            /// truncation diameter of the field along y direction
            /// </summary>
            public double DiameterY { get; set; }

            /// <summary>
            /// aboslute edge width (half within, half outside)
            /// </summary>
            public double EdgeWidth {  get; set; }

            /// <summary>
            /// shape of the truncation aperture
            /// </summary>
            public ApertureShape Shape { get; set; }  

            /// <summary>
            /// lateral shift of the field along x direction
            /// </summary>
            public double ShiftX {  get; set; }

            /// <summary>
            /// lateral shift of the field along y direction
            /// </summary>
            public double ShiftY { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling {  get; set; }

            /// <summary>
            /// shift in the spatial frequency domain along kx
            /// i.e. the linear phase in spatial domain along x
            /// </summary>
            public double Kx0 {  get; set; }

            /// <summary>
            /// shift in the spatial frequency domain along ky
            /// i.e. the linear phase in spatial domain along y
            /// </summary>
            public double Ky0 {  get; set; }

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
            /// <param name="diameterX"> diameter of the truncation along x direction </param>
            /// <param name="diameterY"> diameter of the truncation along y direction </param>
            /// <param name="grid"> 2D uniform sampling grid </param>
            /// <param name="shape"> shape of the truncation aperture </param>
            /// <param name="edge"> absolute edge width (half within, half outside) </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase along x direction </param>
            /// <param name="ky0"> spatial frequency that defines the linear phase along y direction </param>
            public PlaneWave(double wavelength, Complex n,
                double diameterX, double diameterY, GridInfo2D grid,
                ApertureShape shape = ApertureShape.Elliptical,
                double edge = 0.0,
                double shiftX = 0.0, double shiftY = 0.0, double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0, double ky0 = 0.0)
                : base(wavelength: wavelength, n: n, grid: grid,
                      domain: domain, direction: direction,
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                DiameterX = diameterX;
                DiameterY = diameterY;
                EdgeWidth = edge;
                Shape = shape;
                ShiftX = shiftX;
                ShiftY = shiftY;
                Scaling = scale;
                Kx0 = kx0;
                Ky0 = ky0;

                // fills field values
                switch(Shape)
                {
                    case ApertureShape.Elliptical:
                        {
                            Aperture2D.Ellptical a = new(DiameterX, DiameterY,
                                EdgeWidth, ShiftX, ShiftY, Scaling);
                            Field = a.Sample(grid: Grid, loopMode: loopMode);
                            break;
                        }
                    case ApertureShape.Rectangular:
                        {
                            Aperture2D.Rectangular a = new(DiameterX, DiameterY,
                                EdgeWidth, ShiftX, ShiftY, Scaling);
                            Field = a.Sample(grid: Grid, loopMode: loopMode);
                            break;
                        }
                    default: goto case ApertureShape.Elliptical;
                }
                // handles possible linear phase
                MatrixZ t = Field;
                if (Kx0 != 0.0 || Ky0 != 0.0) { FieldCommons.AddLinearPhase(x: ref t, grid: Grid, c1x: Kx0, c1y: Ky0, loopMode: loopMode); }
            }

            #endregion
        }


        /// <summary>
        /// 2D scalar truncated spherical wave field
        /// </summary>
        public class SphericalWave: ScalarField
        {
            #region properties

            /// <summary>
            /// distance from the point source
            /// </summary>
            public double SourceDistance { get; set; }

            /// <summary>
            /// truncation diameter of the field along x direction
            /// </summary>
            public double DiameterX { get; set; }

            /// <summary>
            /// truncation diameter of the field along y direction
            /// </summary>
            public double DiameterY { get; set; }

            /// <summary>
            /// aboslute edge width (half within, half outside)
            /// </summary>
            public double EdgeWidth { get; set; }

            /// <summary>
            /// shape of the truncation aperture
            /// </summary>
            public ApertureShape Shape { get; set; }

            /// <summary>
            /// lateral shift of the field along x direction
            /// </summary>
            public double ShiftX { get; set; }

            /// <summary>
            /// lateral shift of the field along y direction
            /// </summary>
            public double ShiftY { get; set; }

            /// <summary>
            /// real-valued constant scaling factor
            /// </summary>
            public double Scaling { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain along kx
            /// i.e. the linear phase in spatial domain along x
            /// </summary>
            public double Kx0 { get; set; }

            /// <summary>
            /// shift in the spatial frequency domain along ky
            /// i.e. the linear phase in spatial domain along y
            /// </summary>
            public double Ky0 { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// default constructor
            /// </summary>
            public SphericalWave() { }

            /// <summary>
            /// constructs a spherical wave with truncated amplitude
            /// </summary>
            /// <param name="wavelength"> wavelength in vacuum </param>
            /// <param name="n"> complex refractive index @wavelength </param>
            /// <param name="z"> distance from the point source </param>
            /// <param name="alphaX"> half-opening angle along x direction </param>
            /// <param name="alphaY"> half-opening angle along y direction </param>
            /// <param name="grid"> 2D uniform sampling grid </param>
            /// <param name="shape"> shape of the truncation aperture </param>
            /// <param name="edge"> absolute edge width (half within, half outside) </param>
            /// <param name="shiftX"> lateral shift of the field along x direction </param>
            /// <param name="shiftY"> lateral shift of the field along y direction </param>
            /// <param name="scale"> real-valued constant scaling factor </param>
            /// <param name="domain"> modeling domain in which the field is specified </param>
            /// <param name="direction"> sign factor for the direction </param>
            /// <param name="initializeEigenInfo"> whether to compute the eigen info when constructed </param>
            /// <param name="loopMode"> computational option for loops </param>
            /// <param name="kx0"> spatial frequency that defines the linear phase along x direction </param>
            /// <param name="ky0"> spatial frequency that defines the linear phase along y direction </param>
            /// <exception cref="NotImplementedException"></exception>
            public SphericalWave(double wavelength, Complex n,
                double z, double alphaX, double alphaY, GridInfo2D grid,
                ApertureShape shape = ApertureShape.Elliptical,
                double edge = 0.0, 
                double shiftX = 0.0, double shiftY = 0.0, 
                double scale = 1.0,
                ModelingDomain domain = ModelingDomain.Spatial,
                SignFactor direction = SignFactor.Positive,
                bool initializeEigenInfo = true,
                LoopMode loopMode = Defaults.LoopOption,
                double kx0 = 0.0, double ky0 = 0.0)
                : base(wavelength: wavelength, n: n, grid: grid,
                      domain: domain, direction: direction,
                      initializeEigenInfo: initializeEigenInfo)
            {
                // specific information
                SourceDistance = Math.Abs(z);
                DiameterX = 2.0 * Math.Tan(alphaX) * SourceDistance;
                DiameterY = 2.0 * Math.Tan(alphaY) * SourceDistance;
                EdgeWidth = edge;
                Shape = shape;
                ShiftX = shiftX;
                ShiftY = shiftY;
                Scaling = scale;
                Kx0 = kx0;
                Ky0 = ky0;

                // fills the field values
                switch(Shape)
                {
                    case ApertureShape.Elliptical:
                        {
                            Aperture2D.Ellptical a = new(DiameterX, DiameterY,
                                EdgeWidth, ShiftX, ShiftY, Scaling);
                            Field = a.Sample(grid: Grid, loopMode: loopMode);
                            break;
                        }
                    case ApertureShape.Rectangular:
                        {
                            Aperture2D.Rectangular a = new(DiameterX, DiameterY,
                                EdgeWidth, ShiftX, ShiftY, Scaling);
                            Field = a.Sample(grid: Grid, loopMode: loopMode);
                            break;
                        }
                    default: goto case ApertureShape.Elliptical;
                }
                Complex preFac = Math.Sign(z) * Complex.ImaginaryOne * K0 * RefractiveIndex;
                switch (loopMode)
                {
                    case LoopMode.Sequential:
                        {
                            for (long i = 0; i < Grid.Rows * Grid.Cols; i++)
                            {
                                long iRow = i / Grid.Cols;
                                long iCol = i % Grid.Cols;
                                double y = Grid.GetCoordinateY(iRow);
                                double x = Grid.GetCoordinateX(iCol);
                                double r = Math.Sqrt(x * x + y * y + SourceDistance * SourceDistance);
                                Field[iRow, iCol, false] *= SourceDistance / r
                                    * Complex.Exp(preFac * r);
                            }
                            break;
                        }
                    case LoopMode.Parallel:
                        {
                            Parallel.For(0, Grid.Rows * Grid.Cols, i =>
                            {
                                long iRow = i / Grid.Cols;
                                long iCol = i % Grid.Cols;
                                double y = Grid.GetCoordinateY(iRow);
                                double x = Grid.GetCoordinateX(iCol);
                                double r = Math.Sqrt(x * x + y * y + SourceDistance * SourceDistance);
                                Field[iRow, iCol, false] *= SourceDistance / r
                                    * Complex.Exp(preFac * r);
                            });
                            break;
                        }
                    case LoopMode.Vectorized:
                        throw new NotImplementedException();
                    default: goto case LoopMode.Sequential;
                }
                //// handles possible linear phase
                //if (Kx0 != 0.0 || Ky0 != 0.0) { FieldCommons.AddLinearPhase(v: Field, grid: Grid, c1x: Kx0, c1y: Ky0, loopMode: loopMode); }
            }

            #endregion
        }


        #endregion
    }

}
