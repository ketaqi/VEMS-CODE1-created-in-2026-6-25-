using System.Security.Cryptography;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// grid-based 2D scalar fields
    /// </summary>
    public class Grid2DSCFields
    {
        #region properties

        /// <summary>
        /// Gets or sets the type of grid used for field calculations.
        /// </summary>
        public FieldGridType GridType { get; set; } = FieldGridType.Spatial;

        /// <summary>
        /// grid points as the centers of
        /// all the scalar fields
        /// </summary>
        public GridInfo2D GridPoints { get; set; }

        ///// <summary>
        ///// list of scalar fields
        ///// </summary>
        //public Dictionary<(long, long), SCField>? SCFields { get; set; }

        /// <summary>
        /// Gets or sets a two-dimensional array of scalar fields.
        /// </summary>
        public SCField[,]? Fields { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal Grid2DSCFields() 
        {
            GridPoints = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2DSCFields"/> class, representing a grid-based collection
        /// of scalar fields.
        /// </summary>
        /// <param name="gridPoints">The grid points that define the centers of the scalar fields.</param>
        /// <param name="fields">An optional two-dimensional array of scalar fields. If <see langword="null"/>, the grid will be initialized
        /// without predefined fields.</param>
        public Grid2DSCFields(GridInfo2D gridPoints,
            SCField[,]? fields = null)
        {
            GridPoints = gridPoints;
            Fields = fields;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2DSCFields"/> class, constructing a grid-based
        /// collection of scalar fields using a unit field and an optional weights distribution.
        /// </summary>
        /// <param name="gridPoints">The grid points representing the centers of the scalar fields.</param>
        /// <param name="unitField">The unit field to be copied and shifted for each grid point.</param>
        /// <param name="gridType">The type of grid used for field calculations. Defaults to <see cref="FieldGridType.Spatial"/>.</param>
        /// <param name="weights">
        /// Optional weights distribution for all the modes. If provided, must match the grid size.
        /// Each field's <c>Scaling</c> property will be set to the corresponding weight.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="weights"/> is not null and its dimensions do not match <paramref name="gridPoints"/>.
        /// </exception>
        public Grid2DSCFields(GridInfo2D gridPoints,
            SCField unitField,
            FieldGridType gridType = FieldGridType.Spatial,
            MatrixZ? weights = null)
        {
            if (weights != null && (weights.Rows != gridPoints.Rows || weights.Cols != gridPoints.Cols))
            { throw new ArgumentException($"{nameof(weights)} must match the number of {nameof(gridPoints)}"); }

            GridType = gridType;
            GridPoints = gridPoints;
            Fields = new SCField[gridPoints.Rows, gridPoints.Cols];

            for (long iRow = 0; iRow < gridPoints.Rows; iRow++)
            {
                for (long iCol = 0; iCol < gridPoints.Cols; iCol++)
                {
                    // copies the unit field
                    SCField vi = new(other: unitField, copyMode: ArrayCopyMode.Deep);

                    // parameters
                    double k0 = vi.K0;
                    double nRe = vi.Material.NReal(vi.Wavelength);

                    // shifts ...
                    double shiftX, shiftY;
                    double shiftKx, shiftKy;
                    switch (GridType)
                    {
                        case FieldGridType.Spatial:
                            shiftX = gridPoints.GetCoordinateX(iCol);
                            shiftY = gridPoints.GetCoordinateY(iRow);
                            shiftKx = 0.0;
                            shiftKy = 0.0;
                            break;
                        case FieldGridType.Angular:
                            shiftX = 0.0;
                            shiftY = 0.0;
                            shiftKx = k0 * nRe * Math.Sin(gridPoints.GetCoordinateX(iCol));
                            shiftKy = k0 * nRe * Math.Sin(gridPoints.GetCoordinateY(iRow));
                            break;
                        case FieldGridType.NumericalAperture:
                            shiftX = 0.0;
                            shiftY = 0.0;
                            shiftKx = k0 * nRe * gridPoints.GetCoordinateX(iCol);
                            shiftKy = k0 * nRe * gridPoints.GetCoordinateY(iRow);
                            break;
                        default: goto case FieldGridType.Spatial;
                    }
                    vi.ShiftX = shiftX;
                    vi.ShiftY = shiftY;
                    vi.ShiftKx = shiftKx;
                    vi.ShiftKy = shiftKy;

                    // weights
                    if (weights != null)
                    { vi.Scaling = weights[iRow, iCol, false]; }

                    // add to the fields array
                    Fields[iRow, iCol] = vi;
                }
            }
        }

        #endregion
        #region methods

        // ...

        private void T() { }


        /// <summary>
        /// Initializes the weights (scaling factors) of all scalar fields in the grid
        /// according to the specified aperture shape and size.
        /// </summary>
        /// <param name="diameterX">The diameter of the aperture along the x-direction.</param>
        /// <param name="diameterY">The diameter of the aperture along the y-direction.</param>
        /// <param name="shape">
        /// The shape of the aperture used for weight calculation.
        /// <see cref="ApertureShape.Elliptical"/> applies an elliptical aperture (default).
        /// <see cref="ApertureShape.Rectangular"/> applies a rectangular aperture.
        /// </param>
        /// <param name="edge">
        /// The edge width for the aperture transition (absolute, half inside and half outside the aperture).
        /// Used for smoothing the aperture edge.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <see cref="Fields"/> array is <c>null</c>.
        /// </exception>
        public void InitializeWeights(double diameterX, double diameterY,
            ApertureShape shape = ApertureShape.Elliptical, double edge = 0.0)
        {
            if (Fields == null) { throw new ArgumentNullException(nameof(Fields)); }

            Func<double, double, double> a;
            switch (shape)
            {
                case ApertureShape.Elliptical:
                    a = (x, y) => Function2D.CosEdgeEllipse(x, y,
                        [0.5 * diameterX, 0.5 * diameterY, edge]);
                    break;
                case ApertureShape.Rectangular:
                    a = (x, y) =>
                    Function1D.CosEdgeRectangle(x, [diameterX, edge]) *
                    Function1D.CosEdgeRectangle(y, [diameterY, edge]);
                    break;
                default: goto case ApertureShape.Elliptical;
            }

            // defines weights
            for (long iRow = 0; iRow < GridPoints.Rows; iRow++)
            {
                for (long iCol = 0; iCol < GridPoints.Cols; iCol++)
                {
                    double x = GridPoints.GetCoordinateX(iCol);
                    double y = GridPoints.GetCoordinateY(iRow);
                    double w = a(x, y);
                    Fields[iRow, iCol].Scaling = w;
                }
            }
        }

        #endregion
    }
}
