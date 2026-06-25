using Complex = System.Numerics.Complex;

namespace VEMS.MathCore
{

    /// <summary>
    /// inner product of two real-valued 1D functions
    /// </summary>
    public class Product1DReal
    {
        #region properties

        /// <summary>
        /// source function
        /// <para> variable: x </para>
        /// <para> function value: f = f(x) </para>
        /// </summary>
        public Func<double, double> Source { get; set; }

        /// <summary>
        /// target function
        /// <para> variable: x </para>
        /// <para> function value: t = t(x) </para>
        /// </summary>
        public Func<double, double> Target { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs an InnerProduct class
        /// </summary>
        /// <param name="source"> source function </param>
        /// <param name="target"> target function </param>
        public Product1DReal(Func<double, double> source, 
            Func<double, double> target)
        {
            Source = source;
            Target = target;
        }

        /// <summary>
        /// constructs an InnerProduct class
        /// </summary>
        /// <param name="source"> source function (as GridData) </param>
        /// <param name="target"> target function (as GridData) </param>
        public Product1DReal(Grid1DRealData source,
            Grid1DRealData target)
            : this(source.FindValue, target.FindValue) 
        { }

        /// <summary>
        /// constructs an InnerProduct class
        /// </summary>
        /// <param name="source"> source function </param>
        /// <param name="target"> target function (as GridData) </param>
        public Product1DReal(Func<double, double> source,
            Grid1DRealData target)
            : this(source, target.FindValue) 
        { }

        /// <summary>
        /// constructs an InnerProduct class
        /// </summary>
        /// <param name="source"> source function (as GridData) </param>
        /// <param name="target"> target function </param>
        public Product1DReal(Grid1DRealData source,
            Func<double, double> target)
            : this(source.FindValue, target) 
        { }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the value of inner product 
        /// between source and target function
        /// </summary>
        /// <param name="samples"> number of samples within the integral </param>
        /// <param name="start"> start value of the integral </param>
        /// <param name="end"> end value of the integral </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> value of inner product </returns>
        public double Evaluate(long samples, 
            double start = double.NegativeInfinity,
            double end = double.PositiveInfinity, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            // defines the product function
            Func<double, double> p = (x) => Source(x) * Target(x);
            // defines grid
            double dx = (end - start) / samples;
            GridInfo1D g = new(n: samples, spacing: dx,
                refPoint: start, refType: GridRefType.Start);
            // sampling ...
            Samp1DRealFunc sf = new(f: p);
            VectorD vs = sf.Sample(grid: g, loopMode: loopMode);
            // return 
            return g.Spacing * VMath.Sum(vs, loopMode); // vs.Sum(mode: loopMode);
        }

        #endregion
    }

    /// <summary>
    /// inner product of two complex-valued 1D functions
    /// </summary>
    public class Product1DCplx
    {
        #region properties

        /// <summary>
        /// source function
        /// <para> variable: x </para>
        /// <para> function value: f = f(x) </para>
        /// </summary>
        public Func<double, Complex> Source { get; set; }

        /// <summary>
        /// target function
        /// <para> variable: x </para>
        /// <para> function value: t = t(x) </para>
        /// </summary>
        public Func<double, Complex> Target { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs an InnerProduct class
        /// </summary>
        /// <param name="source"> source function </param>
        /// <param name="target"> target function </param>
        public Product1DCplx(Func<double, Complex> source,
            Func<double, Complex> target)
        {
            Source = source;
            Target = target;
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the value of inner product 
        /// between source and target function
        /// </summary>
        /// <param name="samples"> number of samples within the integral </param>
        /// <param name="start"> start value of the integral </param>
        /// <param name="end"> end value of the integral </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> value of inner product </returns>
        public Complex Evaluate(long samples,
            double start = double.NegativeInfinity,
            double end = double.PositiveInfinity,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // defines the product function
            Func<double, Complex> p = (x) => Source(x) * Complex.Conjugate(Target(x));
            // defines grid
            double dx = (end - start) / samples;
            GridInfo1D g = new(n: samples, spacing: dx,
                refPoint: start, refType: GridRefType.Start);
            // sampling ...
            Samp1DCplxFunc sf = new(f: p);
            VectorZ vs = sf.Sample(grid: g, loopMode: loopMode);
            // return 
            return g.Spacing * VMath.Sum(vs, loopMode); //vs.Sum(mode: loopMode);
        }

        #endregion
    }

    /// <summary>
    /// inner product of two real-valued 2D functions
    /// </summary>
    public class Product2DReal
    {
        #region properties

        /// <summary>
        /// source function
        /// <para> variable #1: x </para>
        /// <para> variable #2: y </para>
        /// <para> function value: f = f(x, y) </para>
        /// </summary>
        public Func<double, double, double> Source { get; set; }

        /// <summary>
        /// target function
        /// <para> variable #1: x </para>
        /// <para> variable #2: y </para>
        /// <para> function value: t = t(x, y) </para>
        /// </summary>
        public Func<double, double, double> Target { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public Product2DReal(Func<double, double, double> source,
            Func<double, double, double> target)
        {
            Source = source; 
            Target = target;
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the value of inner product 
        /// between source and target function
        /// </summary>
        /// <param name="rowSamples"> number of samples along row direction </param>
        /// <param name="colSamples"> number of samples along column direction </param>
        /// <param name="rowStart"> start value of the integral along row direction </param>
        /// <param name="rowEnd"> end value of the integral along row direction </param>
        /// <param name="colStart"> start value of the integral along column direction </param>
        /// <param name="colEnd"> end value of the integral along column direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> value of inner product </returns>
        public double Evaluate(long rowSamples, long colSamples,
            double rowStart = double.NegativeInfinity, double rowEnd = double.PositiveInfinity,
            double colStart = double.NegativeInfinity, double colEnd = double.PositiveInfinity,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // defines the product function
            Func<double, double, double> p = (x, y) => Source(x, y) * Target(x, y);
            // defines grid
            double dy = (rowEnd - rowStart) / rowSamples;
            double dx = (colEnd - colStart) / colSamples;
            GridInfo2D g = new(rows: rowSamples, cols: colSamples,
                spacingY: dy, refPointY: rowStart, refTypeY: GridRefType.Start,
                spacingX: dx, refPointX: colStart, refTypeX: GridRefType.Start);
            // sampling ...
            Samp2DRealFunc sf = new(f: p);
            MatrixD vs = sf.Sample(grid: g, loopMode: loopMode);
            // return 
            return g.SpacingY * g.SpacingX * VMath.Sum(vs, loopMode); //vs.Sum(mode: loopMode);
        }

        #endregion
    }

    /// <summary>
    /// inner product of two complex-valued 2D functions
    /// </summary>
    public class Product2DCplx
    {
        #region properties

        /// <summary>
        /// source function
        /// <para> variable #1: x </para>
        /// <para> variable #2: y </para>
        /// <para> function value: f = f(x, y) </para>
        /// </summary>
        public Func<double, double, Complex> Source { get; set; }

        /// <summary>
        /// target function
        /// <para> variable #1: x </para>
        /// <para> variable #2: y </para>
        /// <para> function value: t = t(x, y) </para>
        /// </summary>
        public Func<double, double, Complex> Target { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public Product2DCplx(Func<double, double, Complex> source,
            Func<double, double, Complex> target)
        {
            Source = source;
            Target = target;
        }

        #endregion
        #region methods

        /// <summary>
        /// evaluates the value of inner product 
        /// between source and target function
        /// </summary>
        /// <param name="rowSamples"> number of samples along row direction </param>
        /// <param name="colSamples"> number of samples along column direction </param>
        /// <param name="rowStart"> start value of the integral along row direction </param>
        /// <param name="rowEnd"> end value of the integral along row direction </param>
        /// <param name="colStart"> start value of the integral along column direction </param>
        /// <param name="colEnd"> end value of the integral along column direction </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> value of inner product </returns>
        public Complex Evaluate(long rowSamples, long colSamples,
            double rowStart = double.NegativeInfinity, double rowEnd = double.PositiveInfinity,
            double colStart = double.NegativeInfinity, double colEnd = double.PositiveInfinity,
            LoopMode loopMode = Defaults.LoopOption)
        {
            // defines the product function
            Func<double, double, Complex> p = (x, y) => Source(x, y) * Target(x, y);
            // defines grid
            double dy = (rowEnd - rowStart) / rowSamples;
            double dx = (colEnd - colStart) / colSamples;
            GridInfo2D g = new(rows: rowSamples, cols: colSamples,
                spacingY: dy, refPointY: rowStart, refTypeY: GridRefType.Start,
                spacingX: dx, refPointX: colStart, refTypeX: GridRefType.Start);
            // sampling ...
            Samp2DCplxFunc sf = new(f: p);
            MatrixZ vs = sf.Sample(grid: g, loopMode: loopMode);
            // return 
            return g.SpacingY * g.SpacingX * VMath.Sum(vs, loopMode); // vs.Sum(mode: loopMode);
        }

        #endregion
    }

}

