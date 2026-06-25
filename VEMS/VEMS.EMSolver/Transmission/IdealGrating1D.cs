using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// one-dimensional idealized gratings
    /// either amplitude or phase transmission
    /// </summary>
    public class IdealGrating1D : Transmission1D
    {
        #region properties

        /// <summary>
        /// period of the grating
        /// </summary>
        public double Period
        {
            get => D;
            set => D = value;
        }

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public IdealGrating1D() { }

        /// <summary>
        /// constructs an idealized grating with given parameters
        /// </summary>
        /// <param name="period"> period of the grating </param>
        /// <param name="shift"> lateral shift </param>
        /// <param name="scaling"> scaling factor </param>
        public IdealGrating1D(double period,
            double shift = 0.0, double scaling = 1.0)
            : base(shift, scaling)
        {
            Period = period;
        }

        #endregion
        #region methods

        #region ---- coefficients ----

        /// <summary>
        /// calculates the diffraction coefficients of the grating
        /// by using Fourier series expansion
        /// </summary>
        /// <param name="targetN"> target number of samples within the period </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result diffraction coefficients (centered around zero) </returns>
        internal VectorZ CalculateOrderCoefficients(long targetN, 
            LoopMode loopMode = Defaults.LoopOption)
            => ComputeKCoefficients(period: Period, nx: targetN,
                loopMode: loopMode);

        /// <summary>
        /// calculates the diffraction coefficients of the grating
        /// by using Fourier series expansion
        /// </summary>
        /// <param name="targetDx"> target sampling distance of the grating transmission </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> result diffraction coefficients (centered around zero) </returns>
        internal VectorZ CalculateOrderCoefficients(double targetDx,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // determines sampling parameters
            long n = (long)Math.Ceiling(Period / targetDx);

            return ComputeKCoefficients(period: Period, nx: n,
                loopMode: loopMode);
        }

        #endregion
        #region ---- k-mod 1D ----

        /// <summary>
        /// modulates on a field with the transmission function
        /// using k-domain algorithm
        /// </summary>
        /// <typeparam name="T"> any sub-class derived from ScalarField1D </typeparam>
        /// <param name="v"> field to be modulated by the transmission function </param>
        /// <param name="orders"> list of diffraction orders to be considered in the k-domain algorithm </param>
        /// <param name="n"> target number of samples within the period </param>
        /// <param name="intrpl"> interpolation method used in the k-domain </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        [Obsolete]
        public void ModulateInKDomain<T>(ref T v, List<int> orders,
            long n = 301, 
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : ScalarField1D
        {
            // null case hanlding
            if (v.Field == null) { throw new ArgumentNullException(nameof(v.Field)); }
            // transforms to k-domain
            v.SwitchToKDomain();

            // computes order coefficients
            VectorZ c = CalculateOrderCoefficients(targetN: n, 
                loopMode: loopMode);
            long ctrIdx = (c.Count - 1) / 2;

            // generates orders
            VectorZ vOut = new(count: v.Grid.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo1D gi = new(other: v.Grid);
                gi.GetModified(ctrShift: orders[i] * 2.0 * Math.PI / Period);
                // samples the current field back to centered coordinate by interpolation
                Grid1DCplxInterpolation itp = new(v: v.Field, grid: gi,
                    method: intrpl,
                    bound: DataBoundary.ConstantZero);
                VectorZ vi = itp.Evaluate(targetGrid: v.Grid,
                    loopMode: loopMode);
                // adds the modulated field
                VMath.ScaleOn(x: ref vi, a: c[ctrIdx + orders[i], false]);
                VMath.AddTo(x: vi, y: ref vOut);
            }

            // updates field for output
            v.Field = vOut;
        }

        /// <summary>
        /// modulates on a field with the transmission function
        /// using k-domain algorithm
        /// </summary>
        /// <typeparam name="T"> any sub-class derived from ScalarField1D </typeparam>
        /// <param name="v"> field to be modulated by the transmission function </param>
        /// <param name="minOrder"> minimum diffraction order considered </param>
        /// <param name="maxOrder"> maximum diffraction order considered </param>
        /// <param name="n"> target number of samples within the period </param>
        /// <param name="intrpl"> interpolation method used in the k-domain </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        [Obsolete]
        public void ModulateInKDomain<T>(ref T v, int minOrder, int maxOrder,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : ScalarField1D
        {
            // determines order range
            List<int> orders = new ();
            for(int i = minOrder; i <= maxOrder; i++)
            { orders.Add(i); }
            // calls method
            ModulateInKDomain(ref v, orders, n, intrpl, loopMode);
        }


        /// <summary>
        /// Modulates a 1D scalar field in the k-domain using the transmission function of the grating.
        /// For each specified diffraction order, creates a new field shifted in the k-domain and scaled by the corresponding Fourier coefficient.
        /// Returns a list of modulated fields, one for each order.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField1D"/> representing a 1D scalar field.</typeparam>
        /// <param name="vIn">Input scalar field to be modulated. The field is not modified in place.</param>
        /// <param name="orders">List of diffraction orders to be considered in the k-domain algorithm.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>List of <see cref="SCField1D"/> instances, each corresponding to a modulated field for a given diffraction order.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>vIn.UValues</c> or <c>vIn.UGrid</c> is <c>null</c>.</exception>
        public List<SCField1D> ModulateInKDomain<T>(T vIn, List<int> orders,
            long n,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField1D
        {
            // null case hanlding
            if (vIn.UValues == null) { throw new ArgumentNullException(nameof(vIn.UValues)); }
            if (vIn.UGrid == null) { throw new ArgumentNullException(nameof(vIn.UGrid)); }

            // initialize output list
            List<SCField1D> vOut = new(capacity: orders.Count);

            // transforms to k-domain
            vIn.SwitchToKDomain();

            // computes order coefficients
            VectorZ c = CalculateOrderCoefficients(targetN: n,
                loopMode: loopMode);
            long ctrIdx = (c.Count - 1) / 2;

            // generates orders
            for (int i = 0; i < orders.Count; i++)
            {
                // copies from input field in the k-domain
                SCField1D vi = new(other: vIn, copyMode: ArrayCopyMode.Deep);
                // shifts in the k-domain
                vi.ShiftKx += orders[i] * 2.0 * Math.PI / Period;
                // scales field values
                if (vi.UValues == null) { throw new ArgumentNullException(nameof(vi.UValues)); }
                VectorZ ui = vi.UValues;
                VMath.ScaleOn(x: ref ui, a: c[ctrIdx + orders[i], false]);
                // adds into output list
                vOut.Add(vi);
            }
            return vOut;
        }


        /// <summary>
        /// Modulates a 1D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed for a range of diffraction orders from <paramref name="minOrder"/> to <paramref name="maxOrder"/> (inclusive).
        /// Returns a list of modulated fields, one for each order.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField1D"/> representing a 1D scalar field.</typeparam>
        /// <param name="vIn">Input scalar field to be modulated. The field is not modified in place.</param>
        /// <param name="minOrder">Minimum diffraction order considered.</param>
        /// <param name="maxOrder">Maximum diffraction order considered.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>List of <see cref="SCField1D"/> instances, each corresponding to a modulated field for a given diffraction order.</returns>
        public List<SCField1D> ModulateInKDomain<T>(T vIn, int minOrder, int maxOrder,
            long n = 301,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField1D
        {
            // determines order range
            List<int> orders = [];
            for (int i = minOrder; i <= maxOrder; i++)
            { orders.Add(i); }
            // calls method
            return ModulateInKDomain(vIn, orders, n, loopMode);
        }


        /// <summary>
        /// Modulates a 1D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed by summing shifted copies of the field in k-space, each weighted
        /// by the corresponding complex coefficient provided in <paramref name="coeffs"/>.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField1D"/> representing a 1D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>UValues</c> will be updated in place with the modulated result.</param>
        /// <param name="orders">List of diffraction orders to be considered. Each entry corresponds to the coefficient at the same index in <paramref name="coeffs"/>.</param>
        /// <param name="coeffs">Complex coefficients applied to each diffraction order. Must have the same count as <paramref name="orders"/>.</param>
        /// <param name="intrpl">Interpolation method used when resampling the shifted field back to the original k-grid. Defaults to <see cref="InterpolationMethod.Linear"/>.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="v"/> is null, or if <c>v.UValues</c> or <c>v.UGrid</c> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="orders"/> and <paramref name="coeffs"/> do not have the same number of elements.
        /// </exception>
        /// <remarks>
        /// The method switches the provided field to the k-domain before performing modulation. It does not switch the field back
        /// to the spatial domain; the caller is responsible for any further domain transformations if required.
        /// </remarks>
        public void ModulateOnInKDomain<T>(ref T v,
            List<int> orders, List<Complex> coeffs,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField1D
        {
            // null case hanlding
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid)); }
            if (orders.Count != coeffs.Count) { throw new ArgumentException("Orders and efficiencies must have the same length."); }
            // transforms to k-domain
            v.SwitchToKDomain();

            // generates orders
            VectorZ vOut = new(count: v.UGrid.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo1D gi = new(other: v.UGrid);
                gi.GetModified(ctrShift: orders[i] * 2.0 * Math.PI / Period);
                // samples the current field back to centered coordinate by interpolation
                Grid1DCplxInterpolation itp = new(v: v.UValues, grid: gi,
                    method: intrpl,
                    bound: DataBoundary.ConstantZero);
                VectorZ vi = itp.Evaluate(targetGrid: v.UGrid,
                    loopMode: loopMode);
                // adds the modulated field
                VMath.ScaleOn(x: ref vi, a: coeffs[i]);
                VMath.AddTo(x: vi, y: ref vOut);
            }

            // updates field for output
            v.UValues = vOut;
        }


        /// <summary>
        /// Modulates a 1D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed by summing over the specified diffraction orders, each order being shifted in the k-domain
        /// and weighted by the corresponding Fourier coefficient of the grating transmission function.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField1D"/> representing a 1D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>Field</c> property will be updated in place.</param>
        /// <param name="orders">List of diffraction orders to be considered in the k-domain algorithm.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="intrpl">Interpolation method used in the k-domain. Default is <see cref="InterpolationMethod.Linear"/>.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>v.Field</c> is <c>null</c>.</exception>
        public void ModulateOnInKDomain<T>(ref T v, List<int> orders,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField1D
        {
            // null case hanlding
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid)); }
            // transforms to k-domain
            v.SwitchToKDomain();

            // computes order coefficients
            VectorZ c = CalculateOrderCoefficients(targetN: n,
                loopMode: loopMode);
            long ctrIdx = (c.Count - 1) / 2;

            // generates orders
            VectorZ vOut = new(count: v.UGrid.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo1D gi = new(other: v.UGrid);
                gi.GetModified(ctrShift: orders[i] * 2.0 * Math.PI / Period);
                // samples the current field back to centered coordinate by interpolation
                Grid1DCplxInterpolation itp = new(v: v.UValues, grid: gi,
                    method: intrpl,
                    bound: DataBoundary.ConstantZero);
                VectorZ vi = itp.Evaluate(targetGrid: v.UGrid,
                    loopMode: loopMode);
                // adds the modulated field
                VMath.ScaleOn(x: ref vi, a: c[ctrIdx + orders[i], false]);
                VMath.AddTo(x: vi, y: ref vOut);
            }
            // updates field for output
            v.UValues = vOut;
        }


        /// <summary>
        /// Modulates a 1D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed by summing over the specified range of diffraction orders, each order being shifted in the k-domain
        /// and weighted by the corresponding Fourier coefficient of the grating transmission function.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField1D"/> representing a 1D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>Field</c> property will be updated in place.</param>
        /// <param name="minOrder">Minimum diffraction order considered.</param>
        /// <param name="maxOrder">Maximum diffraction order considered.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="intrpl">Interpolation method used in the k-domain. Default is <see cref="InterpolationMethod.Linear"/>.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        public void ModulateOnInKDomain<T>(ref T v, int minOrder, int maxOrder,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : SCField1D
        {
            // determines order range
            List<int> orders = [];
            for (int i = minOrder; i <= maxOrder; i++)
            { orders.Add(i); }
            // calls method
            ModulateOnInKDomain(ref v, orders, n, intrpl, loopMode);
        }

        #endregion
        #region ---- k-mod 2D ----

        /// <summary>
        /// modulates on a field with the transmission function
        /// using k-domain algorithm
        /// </summary>
        /// <typeparam name="T"> any sub-class derived from ScalarField </typeparam>
        /// <param name="v"> field to be modulated by the transmission function </param>
        /// <param name="isAlongX"> whether the modulation is along x or y direction </param>
        /// <param name="orders"> list of diffraction orders to be considered in the k-domain algorithm </param>
        /// <param name="n"> target number of samples within the period </param>
        /// <param name="intrpl"> interpolation method used in the k-domain </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        [Obsolete]
        public void ModulateInKDomain<T>(ref T v, bool isAlongX,
            List<int> orders,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
        {
            // null case hanlding
            if (v.Field == null) { throw new ArgumentNullException(nameof(v.Field)); }
            // transforms to k-domain
            v.SwitchToKDomain();

            // computes order coefficients
            VectorZ c = CalculateOrderCoefficients(targetN: n, 
                loopMode: loopMode);
            long ctrIdx = (c.Count - 1) / 2;

            // generates orders
            MatrixZ vOut = new(rows: v.Grid.Rows, cols: v.Grid.Cols);
            for (int i = 0; i < orders.Count; i++)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo2D gi = new(other: v.Grid);
                gi.GetModified(ctrShiftX: isAlongX ? orders[i] * 2.0 * Math.PI / Period : 0.0,
                    ctrShiftY: isAlongX ? 0.0 : orders[i] * 2.0 * Math.PI / Period);
                // samples the current field back to centered coordinate by interpolation
                Grid2DCplxInterpolation itp = new(v: v.Field, grid: gi,
                    method: intrpl,
                    boundX: DataBoundary.ConstantZero, boundY: DataBoundary.ConstantZero);
                MatrixZ vi = itp.Evaluate(targetGrid: v.Grid,
                    loopMode: loopMode);
                // adds the modulated field
                VMath.ScaleOn(x: ref vi, a: c[ctrIdx + orders[i], false]);
                VMath.AddTo(x: vi, y: ref vOut);
            }
            // updates field for output
            v.Field = vOut;
        }

        /// <summary>
        /// modulates on a field with the transmission function
        /// using k-domain algorithm
        /// </summary>
        /// <typeparam name="T"> any sub-class derived from ScalarField </typeparam>
        /// <param name="v"> field to be modulated by the transmission function </param>
        /// <param name="isAlongX"> whether the modulation is along x or y direction </param>
        /// <param name="minOrder"> minimum diffraction order considered </param>
        /// <param name="maxOrder"> maximum diffraction order considered </param>
        /// <param name="n"> target number of samples within the period </param>
        /// <param name="intrpl"> interpolation method used in the k-domain </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        [Obsolete]
        public void ModulateInKDomain<T>(ref T v, bool isAlongX,
            int minOrder, int maxOrder,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : ScalarField
        {
            // determines order range
            List<int> orders = new();
            for (int i = minOrder; i <= maxOrder; i++)
            { orders.Add(i); }
            // calls method
            ModulateInKDomain(ref v, isAlongX, orders, n, intrpl, loopMode);
        }


        /// <summary>
        /// Modulates a 2D scalar field in the k-domain using the transmission function of the grating.
        /// For each specified diffraction order, creates a new field shifted in the k-domain and scaled by the corresponding Fourier coefficient.
        /// Returns a list of modulated fields, one for each order.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField"/> representing a 2D scalar field.</typeparam>
        /// <param name="vIn">Input scalar field to be modulated. The field is not modified in place.</param>
        /// <param name="isAlongX">If <c>true</c>, the modulation is applied along the x-direction; otherwise, along the y-direction.</param>
        /// <param name="orders">List of diffraction orders to be considered in the k-domain algorithm.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>List of <see cref="SCField"/> instances, each corresponding to a modulated field for a given diffraction order.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>vIn.UValues</c> or <c>vIn.UGrid</c> is <c>null</c>.</exception>
        public List<SCField> ModulateInKDomain<T>(T vIn, bool isAlongX,
            List<int> orders,
            long n = 301,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField
        {
            // null case hanlding
            if (vIn.UValues == null) { throw new ArgumentNullException(nameof(vIn.UValues)); }
            if (vIn.UGrid == null) { throw new ArgumentNullException(nameof(vIn.UGrid)); }

            // initialize output list
            List<SCField> vOut = new(capacity: orders.Count);

            // transforms to k-domain
            vIn.SwitchToKDomain();

            // computes order coefficients
            VectorZ c = CalculateOrderCoefficients(targetN: n,
                loopMode: loopMode);
            long ctrIdx = (c.Count - 1) / 2;

            // generates orders
            for (int i = 0; i < orders.Count; i++)
            {
                // copies from input field in the k-domain
                SCField vi = new(other: vIn, copyMode: ArrayCopyMode.Deep);
                // shifts in the k-domain
                if (isAlongX) { vi.ShiftKx += orders[i] * 2.0 * Math.PI / Period; }
                else { vi.ShiftKy += orders[i] * 2.0 * Math.PI / Period; }
                // scales field values
                if (vi.UValues == null) { throw new ArgumentNullException(nameof(vi.UValues)); }
                MatrixZ ui = vi.UValues;
                VMath.ScaleOn(x: ref ui, a: c[ctrIdx + orders[i], false]);
                // adds into output list
                vOut.Add(vi);
            }
            return vOut;
        }


        /// <summary>
        /// Modulates a 2D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed for a range of diffraction orders from <paramref name="minOrder"/> to <paramref name="maxOrder"/> (inclusive).
        /// Returns a list of modulated fields, one for each order.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField"/> representing a 2D scalar field.</typeparam>
        /// <param name="vIn">Input scalar field to be modulated. The field is not modified in place.</param>
        /// <param name="isAlongX">If <c>true</c>, the modulation is applied along the x-direction; otherwise, along the y-direction.</param>
        /// <param name="minOrder">Minimum diffraction order considered.</param>
        /// <param name="maxOrder">Maximum diffraction order considered.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <returns>List of <see cref="SCField"/> instances, each corresponding to a modulated field for a given diffraction order.</returns>
        public List<SCField> ModulateInKDomain<T>(T vIn, bool isAlongX,
            int minOrder, int maxOrder,
            long n = 301,
            LoopMode loopMode = Defaults.LoopOption) 
            where T : SCField
        {
            // determines order range
            List<int> orders = [];
            for (int i = minOrder; i <= maxOrder; i++)
            { orders.Add(i); }
            // calls method
            return ModulateInKDomain(vIn, isAlongX, orders, n, loopMode);
        }


        /// <summary>
        /// Modulates a 2D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed by summing over the specified diffraction orders, each order being shifted in the k-domain
        /// and weighted by the corresponding Fourier coefficient of the grating transmission function.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField"/> representing a 2D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>Field</c> property will be updated in place.</param>
        /// <param name="isAlongX">If <c>true</c>, the modulation is applied along the x-direction; otherwise, along the y-direction.</param>
        /// <param name="orders">List of diffraction orders to be considered in the k-domain algorithm.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="intrpl">Interpolation method used in the k-domain. Default is <see cref="InterpolationMethod.Linear"/>.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>v.Field</c> is <c>null</c>.</exception>
        public void ModulateOnInKDomain<T>(ref T v, bool isAlongX,
            List<int> orders,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : SCField
        {
            // null case hanlding
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues)); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid)); }
            // transforms to k-domain
            v.SwitchToKDomain();

            // computes order coefficients
            VectorZ c = CalculateOrderCoefficients(targetN: n,
                loopMode: loopMode);
            long ctrIdx = (c.Count - 1) / 2;

            // generates orders
            MatrixZ vOut = new(rows: v.UGrid.Rows, cols: v.UGrid.Cols);
            for (int i = 0; i < orders.Count; i++)
            {
                // makes a new grid that is shifted in k-domain
                GridInfo2D gi = new(other: v.UGrid);
                gi.GetModified(ctrShiftX: isAlongX ? orders[i] * 2.0 * Math.PI / Period : 0.0,
                    ctrShiftY: isAlongX ? 0.0 : orders[i] * 2.0 * Math.PI / Period);
                // samples the current field back to centered coordinate by interpolation
                Grid2DCplxInterpolation itp = new(v: v.UValues, grid: gi,
                    method: intrpl,
                    boundX: DataBoundary.ConstantZero, boundY: DataBoundary.ConstantZero);
                MatrixZ vi = itp.Evaluate(targetGrid: v.UGrid,
                    loopMode: loopMode);
                // adds the modulated field
                VMath.ScaleOn(x: ref vi, a: c[ctrIdx + orders[i], false]);
                VMath.AddTo(x: vi, y: ref vOut);
            }
            // updates field for output
            v.UValues = vOut;
        }

        /// <summary>
        /// Modulates a 2D scalar field in the k-domain using the transmission function of the grating.
        /// The modulation is performed by summing over the specified range of diffraction orders, each order being shifted in the k-domain
        /// and weighted by the corresponding Fourier coefficient of the grating transmission function.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="SCField"/> representing a 2D scalar field.</typeparam>
        /// <param name="v">Reference to the scalar field to be modulated. The field's <c>Field</c> property will be updated in place.</param>
        /// <param name="isAlongX">If <c>true</c>, the modulation is applied along the x-direction; otherwise, along the y-direction.</param>
        /// <param name="minOrder">Minimum diffraction order considered.</param>
        /// <param name="maxOrder">Maximum diffraction order considered.</param>
        /// <param name="n">Target number of samples within the period for Fourier coefficient calculation. Default is 301.</param>
        /// <param name="intrpl">Interpolation method used in the k-domain. Default is <see cref="InterpolationMethod.Linear"/>.</param>
        /// <param name="loopMode">Loop-computational mode options for performance tuning. Default is <see cref="Defaults.LoopOption"/>.</param>
        public void ModulateOnInKDomain<T>(ref T v, bool isAlongX,
            int minOrder, int maxOrder,
            long n = 301,
            InterpolationMethod intrpl = InterpolationMethod.Linear,
            LoopMode loopMode = Defaults.LoopOption) where T : SCField
        {
            // determines order range
            List<int> orders = [];
            for (int i = minOrder; i <= maxOrder; i++)
            { orders.Add(i); }
            // calls method
            ModulateOnInKDomain(ref v, isAlongX, orders, n, intrpl, loopMode);
        }

        #endregion

        #endregion
        #region derived

        /// <summary>
        /// idealized grating with rectangular modulation
        /// </summary>
        public class Rectangular : IdealGrating1D
        {
            #region properties

            /// <summary>
            /// full-width of the rectangle
            /// </summary>
            public double Width { get; set; }

            /// <summary>
            /// absolute edge width of the rectangle (half within, half outside)
            /// </summary>
            public double EdgeWidth { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// constructs an idealized grating with rectangular modulation
            /// </summary>
            /// <param name="period"> period of the grating </param>
            /// <param name="width"> (full) width of the rectangle </param>
            /// <param name="edgeWidth"> (absolute) width of the rectangle </param>
            /// <param name="shift"> lateral shift </param>
            /// <param name="scaling"> scaling factor </param>
            /// <param name="type"> type of the grating modulation: amplitude or phase </param>
            public Rectangular(double period,
                double width, double edgeWidth = 0.0,
                double shift = 0.0, double scaling = 1.0,
                TransmissionType type = TransmissionType.Amplitude)
                : base(period, shift, scaling)
            {
                Width = width;
                EdgeWidth = edgeWidth;
                // defines amplitude and phase
                switch (type)
                {
                    case TransmissionType.Amplitude:
                        {
                            Amplitude = (x) => Scaling * Function1D.CosEdgeRectangle(x, new() { Width, EdgeWidth });
                            Phase = (x) => 0.0;
                            break;
                        }
                    case TransmissionType.Phase:
                        {
                            Amplitude = (x) => 1.0;
                            Phase = (x) => Scaling * Function1D.CosEdgeRectangle(x, new() { Width, EdgeWidth });
                            break;
                        }
                    default: goto case TransmissionType.Amplitude;
                }
            }

            #endregion
        }

        /// <summary>
        /// idealized grating with sinusoidal modulation
        /// </summary>
        public class Sinusoidal : IdealGrating1D
        {
            #region properties

            // ...

            #endregion
            #region constructor

            /// <summary>
            /// constructs an idealized grating with sinusoidal modulation
            /// </summary>
            /// <param name="period"> period of the grating </param>
            /// <param name="shift"> lateral shift </param>
            /// <param name="scaling"> scaling factor </param>
            /// <param name="type"> type of the grating modulation: amplitude or phase </param>
            public Sinusoidal(double period,
                double shift = 0.0, double scaling = 1.0,
                TransmissionType type = TransmissionType.Amplitude)
                : base(period, shift, scaling)
            {
                // defines amplitude and phase
                switch (type)
                {
                    case TransmissionType.Amplitude:
                        {
                            Amplitude = (x) => Scaling * (1.0 + Math.Sin(2.0 * Math.PI * x / Period));
                            Phase = (x) => 0.0;
                            break;
                        }
                    case TransmissionType.Phase:
                        {
                            Amplitude = (x) => 1.0;
                            Phase = (x) => Scaling * Math.Sin(2.0 * Math.PI * x / Period);
                            break;
                        }
                    default: goto case TransmissionType.Amplitude;
                }
            }

            #endregion
        }

        #endregion
    }
}
