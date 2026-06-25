using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents grid-based 1D scalar fields, where each field is defined on a set of grid points.
    /// Provides constructors for various initialization scenarios and supports different grid types.
    /// </summary>
    public class Grid1DSCFields
    {
        #region properties

        /// <summary>
        /// Gets or sets the type of grid used for field calculations.
        /// </summary>
        public FieldGridType GridType { get; set; } = FieldGridType.Spatial;

        /// <summary>
        /// Gets or sets the grid points that serve as the centers of all the 1D scalar fields.
        /// </summary>
        public GridInfo1D GridPoints { get; set; }

        /// <summary>
        /// Gets or sets the collection of 1D scalar fields.
        /// </summary>
        public SCField1D[]? Fields { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid1DSCFields"/> class with default parameters.
        /// </summary>
        internal Grid1DSCFields()
        {
            GridPoints = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid1DSCFields"/> class with specified grid points and optional fields.
        /// </summary>
        /// <param name="gridPoints">The grid points serving as the centers of the fields.</param>
        /// <param name="fields">The array of 1D scalar fields. If <c>null</c>, the field array is not initialized.</param>
        public Grid1DSCFields(GridInfo1D gridPoints,
            SCField1D[]? fields = null)
        {
            GridPoints = gridPoints;
            Fields = fields;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid1DSCFields"/> class with a specific unit field and weights distribution.
        /// Each grid point is associated with a copy of the unit field, shifted and scaled as specified.
        /// </summary>
        /// <param name="gridPoints">The grid points serving as the centers of the fields.</param>
        /// <param name="unitField">The unit field to be copied and shifted for each grid point.</param>
        /// <param name="gridType">The type of grid used for field calculations. Default is <see cref="FieldGridType.Spatial"/>.</param>
        /// <param name="weights">Optional weights distribution for all the modes. If provided, must match the number of grid points.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="weights"/> is not null and its count does not match the number of grid points.</exception>
        public Grid1DSCFields(GridInfo1D gridPoints,
            SCField1D unitField,
            FieldGridType gridType = FieldGridType.Spatial,
            VectorZ? weights = null)
        {
            if (weights != null && weights.Count != gridPoints.Count)
            { throw new ArgumentException($"{nameof(weights)} must match the number of {nameof(gridPoints)}"); }

            GridType = gridType;
            GridPoints = gridPoints;
            Fields = new SCField1D[gridPoints.Count];

            for (int i = 0; i < gridPoints.Count; i++)
            {
                // copies the field
                SCField1D vi = new(other: unitField, copyMode: ArrayCopyMode.Deep);

                // parameters
                double k0 = vi.K0;
                double nRe = vi.Material.NReal(vi.Wavelength);

                // shifts ...
                double shiftX;
                double shiftKx;
                switch (GridType)
                {
                    case FieldGridType.Spatial:
                        shiftX = gridPoints[i];
                        shiftKx = 0.0;
                        break;
                    case FieldGridType.Angular:
                        shiftX = 0.0;
                        shiftKx = k0 * nRe * Math.Sin(gridPoints[i]);
                        break;
                    case FieldGridType.NumericalAperture:
                        shiftX = 0.0;
                        shiftKx = k0 * nRe * gridPoints[i];
                        break;
                    default: goto case FieldGridType.Spatial;
                }
                vi.ShiftX = shiftX;
                vi.ShiftKx = shiftKx;

                // weights
                if (weights != null)
                { vi.Scaling = weights[i, false]; }

                // adds into the fields array
                Fields[i] = vi;
            }
        }

        #endregion
        #region methods

        // ...

        #endregion

        #region derived ...

        /// <summary>
        /// Represents a collection of grid-based 1D scalar fields with a Gaussian profile as the elementary field.
        /// </summary>
        public class Gaussian : Grid1DSCFields
        {
            #region properties

            /// <summary>
            /// Gets or sets the waist radius of the Gaussian profile used as the elementary field.
            /// </summary>
            public double WaistRadius { get; set; }

            /// <summary>
            /// Gets or sets the weights distribution of the modes.
            /// </summary>
            public VectorZ? Weights { get; set; }

            #endregion
            #region constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Gaussian"/> class with default parameters.
            /// </summary>
            internal Gaussian() { }

            ///// <summary>
            ///// Initializes a new instance of the <see cref="Gaussian"/> class with a specific unit field and weights distribution.
            ///// </summary>
            ///// <param name="gridPoints">The grid points serving as the centers of the fields.</param>
            ///// <param name="unitField">The unit field as the elementary mode.</param>
            ///// <param name="weights">The weights distribution of all the modes. If provided, must match the number of grid points.</param>
            ///// <exception cref="ArgumentException">Thrown if <paramref name="weights"/> is not null and its count does not match the number of grid points.</exception>
            //public Gaussian(GridInfo1D gridPoints,
            //    SCField1D unitField,
            //    VectorZ? weights = null)
            //{
            //    if (weights != null && weights.Count != gridPoints.Count)
            //    { throw new ArgumentException($"{nameof(weights)} must match the number of {nameof(gridPoints)}"); }

            //    GridPoints = gridPoints;
            //    SCFields = [];

            //    for (int i = 0; i < gridPoints.Count; i++)
            //    {
            //        SCField1D vi = new(other: unitField, copyMode: ArrayCopyMode.Deep)
            //        {
            //            ShiftX = gridPoints[i],
            //            Scaling = weights == null ? 1.0 : weights[i]
            //        };
            //        SCFields.Add(vi);
            //    }
            //}

            #endregion
        }

        #endregion
    }

}
