using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Provides pointwise mapping functionality for 1D data, supporting arbitrary one-to-one coordinate transformations.
    /// </summary>
    public class Mapper1D // <T> where T : struct
    {
        #region properties

        /// <summary>
        /// Gets or sets the one-to-one mapping relation from input to output coordinate.
        /// <para>Variable: input coordinate</para>
        /// <para>Return: output coordinate</para>
        /// </summary>
        public Func<double, double> InOut { get; set; }

        /// <summary>
        /// Gets or sets the one-to-one mapping relation from output to input coordinate.
        /// <para>Variable: output coordinate</para>
        /// <para>Return: input coordinate</para>
        /// </summary>
        public Func<double, double> OutIn { get; set; }

        ///// <summary>
        ///// Pointwise modulation for each spatial frequency when mapped from input to output.
        ///// </summary>
        //internal Func<double, T> H { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper1D"/> class with identity mapping.
        /// </summary>
        internal Mapper1D()
        {
            InOut = (k) => k;
            OutIn = (k) => k;
            //H = (k) => 1.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper1D"/> class with the specified mapping functions.
        /// </summary>
        /// <param name="inOut">One-to-one mapping from input to output coordinate.</param>
        /// <param name="outIn">One-to-one mapping from output to input coordinate.</param>
        public Mapper1D(Func<double, double> inOut,
            Func<double, double> outIn)
        {
            InOut = inOut;
            OutIn = outIn;
            //H = h;
        }

        #endregion
        #region methods

        ///// <summary>
        ///// Applies the mapping to a given function.
        ///// </summary>
        ///// <typeparam name="U">Type of the function output (double or complex).</typeparam>
        ///// <param name="fIn">Input function fIn = fIn(tIn).</param>
        ///// <returns>Output function fOut = fOut(tOut).</returns>
        //public Func<double, U> Apply<U>(Func<double, U> fIn)
        //    where U : struct
        //{
        //    Func<double, U> fOut = (t) => fIn(OutIn(t));
        //    return fOut;
        //}

        /// <summary>
        /// Applies the mapping to the given real-valued grid data.
        /// </summary>
        /// <param name="dIn">Input data to be mapped.</param>
        /// <param name="gOut">Target grid of the output data. If null, uses the input data's grid.</param>
        /// <param name="loopMode">Computational option for loops.</param>
        /// <returns>Output data after applying the mapping.</returns>
        public Grid1DRealData Apply(Grid1DRealData dIn,
            GridInfo1D? gOut = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // keeps sampling grid?
            gOut ??= new(dIn.GridInfo);

            // initializes output values
            VectorD vOut = new(count: gOut.Count, mode: ArrayInitMode.Malloc);
            // defines loop action
            void op(long i)
            {
                double tOut = gOut[i];
                double tIn = OutIn(tOut);
                vOut[i] = dIn.FindValue(tIn);
            }
            Loop1D loop = new(operation: op, start: 0, end: gOut.Count, step: 1);
            loop.Evaluate(loopMode);

            // return output data
            return new Grid1DRealData(values: vOut, gridInfo: gOut,
                intrpl: dIn.IntrplMethod, bound: dIn.Boundary);
        }

        /// <summary>
        /// Applies the mapping to the given complex-valued grid data.
        /// </summary>
        /// <param name="dIn">Input complex data to be mapped.</param>
        /// <param name="gOut">Target grid of the output data. If null, uses the input data's grid.</param>
        /// <param name="loopMode">Computational option for loops.</param>
        /// <returns>Output complex data after applying the mapping.</returns>
        public Grid1DCplxData Apply(Grid1DCplxData dIn,
            GridInfo1D? gOut = null,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // keeps sampling grid?
            gOut ??= new(dIn.GridInfo);

            // initializes output values
            VectorZ vOut = new(count: gOut.Count);
            // defines loop action
            void op(long i)
            {
                double tOut = gOut[i];
                double tIn = OutIn(tOut);
                vOut[i] = dIn.FindValue(tIn);
            }
            Loop1D loop = new(operation: op, start: 0, end: gOut.Count, step: 1);
            loop.Evaluate(loopMode);

            // return output data
            return new Grid1DCplxData(values: vOut, gridInfo: gOut,
                intrpl: dIn.IntrplMethod, bound: dIn.Boundary);
        }

        #endregion
        #region derived-classes

        /// <summary>
        /// Represents a linear mapping for the 1D case, supporting scaling and offset.
        /// </summary>
        public class Linear : Mapper1D //<T> 
        {
            #region properties

            /// <summary>
            /// Gets or sets the coefficient in the linear translation relation in the input-to-output mapping.
            /// </summary>
            public double C1 { get; set; }

            /// <summary>
            /// Gets or sets the constant offset in the input-to-output mapping.
            /// </summary>
            public double Offset { get; set; }

            #endregion
            #region constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="Linear"/> class with default mapping relations.
            /// </summary>
            /// <remarks>The default mapping relations are defined as follows: <list type="bullet">
            /// <item><description><c>InOut</c>: Maps an input value <c>t</c> to an output value using the formula <c>C1
            /// * t + Offset</c>.</description></item> <item><description><c>OutIn</c>: Maps an output value <c>t</c>
            /// back to an input value using the formula <c>(t - Offset) / C1</c>.</description></item> </list> By
            /// default, <c>C1</c> is set to 1.0 and <c>Offset</c> is set to 0.0.</remarks>
            internal Linear()
            {
                C1 = 1.0;
                Offset = 0.0;
                // defines the mapping relations
                InOut = (t) => C1 * t + Offset;
                OutIn = (t) => (t - Offset) / C1;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Linear"/> class for 1D linear mapping.
            /// </summary>
            /// <param name="c1">Coefficient in the linear translation relation in the input-to-output mapping.</param>
            /// <param name="offset">Constant offset in the input-to-output mapping.</param>
            public Linear(double c1, double offset = 0.0)
            {
                C1 = c1;
                Offset = offset;
                // defines the mapping relations
                InOut = (t) => C1 * t + Offset;
                OutIn = (t) => (t - Offset) / C1;
            }

            #endregion
            #region methods

            /// <summary>
            /// Applies the specific linear mapping to the given 1D real-valued grid data.
            /// Updates the grid information, reverses the data values if the mapping is negative, and scales the values if needed.
            /// </summary>
            /// <param name="d">Reference to the <see cref="Grid1DRealData"/> instance to be modified by the mapping.</param>
            /// <param name="scal">Additional scaling of the values. If <c>null</c>, it is automatically determined as the inverse of the absolute value of the linear coefficient.
            /// </param>            
            public void Apply(ref Grid1DRealData d, 
                double? scal = 1.0)
            {
                // defines output grid according to linear mapping
                GridInfo1D g = d.GridInfo;
                long n = g.Count;
                double spx = g.Spacing * Math.Abs(C1);
                double stx = (C1 >= 0.0) ?
                    g.Start * C1 + Offset : g.End * C1 + Offset;
                // updates grid
                d.GridInfo = new GridInfo1D(n: n, spacing: spx, refPoint: stx, refType: GridRefType.Start);
                // reverse values?
                if (C1 < 0.0) { d.Values.Reverse(); }
                // scales values if needed
                scal ??= 1.0 / Math.Abs(C1);
                if (scal != 1.0)
                {
                    VectorD t = d.Values;
                    //VMath.ScaleOnD(x: ref t, a: scal.Value);
                    VMath.ScaleOn(x: ref t, a: scal.Value);
                }
            }

            /// <summary>
            /// Applies the specific linear mapping to the given 1D complex-valued grid data.
            /// Updates the grid information, reverses the data values if the mapping is negative, scales the values if needed, and updates the phase.
            /// </summary>
            /// <param name="d">Reference to the <see cref="Grid1DCplxData"/> instance to be modified by the mapping.</param>
            /// <param name="scal">
            /// Additional scaling of the values. If <c>null</c>, it is automatically determined as the inverse of the absolute value of the linear coefficient.
            /// </param>
            public void Apply(ref Grid1DCplxData d,
                double? scal = 1.0)
            {
                // defines output grid according to linear mapping
                GridInfo1D g = d.GridInfo;
                long n = g.Count;
                double spx = g.Spacing * Math.Abs(C1);
                double stx = (C1 >= 0.0) ?
                    g.Start * C1 + Offset : g.End * C1 + Offset;
                // updates grid
                d.GridInfo = new GridInfo1D(n: n, spacing: spx, refPoint: stx, refType: GridRefType.Start);
                // reverse values?
                if (C1 < 0.0) { d.Values.Reverse(); }
                // scales values if needed
                scal ??= 1.0 / Math.Abs(C1);
                if (scal != 1.0)
                {
                    VectorZ t = d.Values;
                    //VMath.ScaleOnZ(x: ref t, a: scal.Value);
                    VMath.ScaleOn(x: ref t, a: scal.Value);
                }
                // updates phase
                d.Phase.C1 /= C1;
            }

            #endregion
        }

        // ...

        #endregion
    }


    /// <summary>
    /// Represents an idealized 1D imaging system that applies a linear mapping and a transmission modulation
    /// to a 1D scalar field in the spatial-frequency (k) domain.
    /// </summary>
    public class IdealizedImaging1D
    {
        #region properties

        /// <summary>
        /// Gets or sets the linear translation mapping applied in the k-domain.
        /// </summary>
        public Mapper1D.Linear Translation { get; set; }

        /// <summary>
        /// Gets or sets the transmission modulation applied in the k-domain.
        /// </summary>
        public Transmission1D Modulation { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// Default constructor for the <see cref="IdealizedImaging1D"/> class.
        /// </summary>
        internal IdealizedImaging1D()
        {
            Translation = new Mapper1D.Linear();
            Modulation = new Transmission1D();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdealizedImaging1D"/> class, representing an idealized 1D imaging system.
        /// Applies a linear mapping and a transmission modulation to a 1D scalar field in the spatial-frequency (k) domain.
        /// </summary>
        /// <param name="m">Magnification factor for the imaging in the x-domain.</param>
        /// <param name="offset">Constant offset applied in the linear mapping (default is 0.0).</param>
        /// <param name="amplitude">Optional amplitude modulation function in the k-domain. If null, uses unity amplitude.</param>
        /// <param name="phase">Optional phase modulation function in the k-domain. If null, uses zero phase.</param>
        public IdealizedImaging1D(double m, double offset = 0.0,
            Func<double, double>? amplitude = null,
            Func<double, double>? phase = null)
        {
            double c1 = 1.0 / m; // linear coefficient
            Translation = new Mapper1D.Linear(c1, offset);
            Modulation = new()
            {
                Amplitude = amplitude ?? ((kx) => 1.0),
                Phase = phase ?? ((kx) => 0.0)
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// Applies the linear mapping and transmission modulation to a 1D scalar field in the spatial-frequency (k) domain.
        /// <para>
        /// This method first ensures the field is in the k-domain, then applies the linear mapping to the field's grid and values.
        /// If the linear mapping coefficient is negative, the field values are reversed. The phase term is also updated accordingly.
        /// </para>
        /// <para>
        /// The transmission modulation is not applied in this method; use <see cref="Transmission1D.ModulateOn{T}(ref T, LoopMode)"/> for modulation.
        /// </para>
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="SCField1D"/> representing the 1D scalar field.</typeparam>
        /// <param name="v">Reference to the field to be mapped and modulated.</param>
        /// <param name="scal">Additional scaling of the values. If <c>null</c>, it is automatically determined as the inverse of the absolute value of the linear coefficient.</param>
        /// <param name="loopMode">Loop-computational mode options. Default is <see cref="Defaults.LoopOption"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="v"/>'s <c>UValues</c>, <c>UGrid</c>, or <c>UPhase</c> properties are null.
        /// </exception>
        public void ApplyModulateOn<T>(ref T v,
            double? scal = 1.0,
            LoopMode loopMode = Defaults.LoopOption)
            where T : SCField1D
        {
            if (v.UValues == null) { throw new ArgumentNullException(nameof(v.UValues), "UValues cannot be null."); }
            if (v.UGrid == null) { throw new ArgumentNullException(nameof(v.UGrid), "UGrid cannot be null."); }
            if (v.UPhase == null) { throw new ArgumentNullException(nameof(v.UPhase), "UPhase cannot be null."); }

            // makes sure the field is in k-domain
            v.SwitchToKDomain();

            // defines output grid according to linear mapping
            GridInfo1D g = v.UGrid;
            long n = g.Count;
            double spx = g.Spacing * Math.Abs(Translation.C1);
            double stx = (Translation.C1 >= 0.0) ?
                g.Start * Translation.C1 + Translation.Offset
                : g.End * Translation.C1 + Translation.Offset;
            // updates grid
            v.UGrid = new GridInfo1D(n: n, spacing: spx, refPoint: stx, refType: GridRefType.Start);
            //reverse values?
            if (Translation.C1 < 0.0) { v.UValues.Reverse(); }
            // scales values
            scal ??= 1.0 / Math.Abs(Translation.C1);
            VectorZ t = v.UValues;
            Complex f(double kx) => Modulation.F(kx) * scal.Value;
            new Samp1DCplxFunc(f: f).ScaleOn(x: ref t, grid: v.UGrid, loopMode: loopMode);
            // updates phase
            v.UPhase.C1 /= Translation.C1;
        }

        #endregion
    }

}
