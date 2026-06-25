using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// base-class of materials
    /// </summary>
    public class Material : IEquatable<Material>
    {
        #region properties

        /// <summary>
        /// minimum (vacuum) wavelength for the dispersion definition 
        /// </summary>
        public double WavelengthMin { get; set; }

        /// <summary>
        /// maximum (vacuum) wavelength for the dispersion definition
        /// </summary>
        public double WavelengthMax { get; set; }

        /// <summary>
        /// real-part of refractive index nRe = nRe(λ)
        /// </summary>
        public Func<double, double> NReal { get; set; }

        /// <summary>
        /// real-part of refractive index nIm = nIm(λ)
        /// </summary>
        public Func<double, double> NImag { get; set; }

        /// <summary>
        /// complex refractive index n = n(λ)
        /// </summary>
        public Func<double, Complex> N { get; set; }

        /// <summary>
        /// permittivity epsilon = f(λ)
        /// </summary>
        public Func<double, Complex> Epsilon { get; set; }

        /// <summary>
        /// permiability mu = f(λ)
        /// </summary>
        public Func<double, Complex>? Mu { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a MaterialBase with specific wavelength range
        /// with default 
        /// </summary>
        /// <param name="wavelengthMin"> lower bound of the wavelength range </param>
        /// <param name="wavelengthMax"> upper bound of the wavelength range </param>
        public Material(double wavelengthMin = 0.0, 
            double wavelengthMax = double.PositiveInfinity)
        {
            // defines wavelength range
            WavelengthMin = wavelengthMin;
            WavelengthMax = wavelengthMax;
            // default refractive index
            NReal = (w) => (w < WavelengthMin || w > WavelengthMax) ?
                double.NaN : 1.0;
            NImag = (w) => (w < WavelengthMin || w > WavelengthMax) ? 
                double.NaN : 0.0;
            // complex refractive index
            N = (w) => new Complex(NReal(w), NImag(w));
            // permittivity
            Epsilon = (w) => N(w) * N(w);
        }

        /// <summary>
        /// (deep) copy constructor
        /// </summary>
        /// <param name="other"> another material as the source </param>
        public Material(Material other)
        {
            // deep copy
            WavelengthMin = other.WavelengthMin;
            WavelengthMax = other.WavelengthMax;
            NReal = other.NReal;
            NImag = other.NImag;
            N = other.N;
            Epsilon = other.Epsilon;
            Mu = other.Mu;
        }

        #endregion
        #region methods

        #region ---- sample ----

        /// <summary>
        /// samples the material property on a set of wavelengths
        /// (λ0, λ1, ... , λn) either uniform or scattered
        /// </summary>
        /// <param name="ws"> samples of wavelength </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> sampled material property </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public VectorZ Sample(ScatInfo1D ws,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (ws[0] < WavelengthMin || ws[ws.Count - 1] > WavelengthMax)
            { throw new ArgumentOutOfRangeException(nameof(ws)); }
            // defines sampler for the material property
            Samp1DCplxFunc sfw;
            switch (matProperty)
            {
                case MaterialProperty.N:
                    sfw = new(f: (w, p) => N(w));
                    break;
                case MaterialProperty.Epsilon:
                    sfw = new(f: (w, p) => Epsilon(w));
                    break;
                case MaterialProperty.Mu:
                    sfw = new(f: (w, p) => Mu(w));
                    break;
                default: goto case MaterialProperty.N;
            }
            // return
            return sfw.Sample(xs: ws, loopMode: loopMode);
        }

        /// <summary>
        /// samples the material property on a target uniform grid
        /// </summary>
        /// <param name="grid"> target uniform grid </param>
        /// <param name="matProperty"> material property option </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> sampled material property </returns>
        public VectorZ Sample(GridInfo1D grid,
            MaterialProperty matProperty = EMDefaults.MatProperty,
            LoopMode loopMode = Defaults.LoopOption)
            => Sample((ScatInfo1D)grid, matProperty, loopMode);

        #endregion
        #region ---- evaluate ----

        /// <summary>
        /// evaluates nReal = nReal(λ0, λ1, ... , λn) on a set of 
        /// wavelengths, either uniform or scattered
        /// </summary>
        /// <param name="w"> samples of wavelength </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> real-parts of refractive indices </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [Obsolete]
        public VectorD EvaluateReal(ScatInfo1D w,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (w[0] < WavelengthMin || w[w.Count - 1] > WavelengthMax)
                throw new ArgumentOutOfRangeException(nameof(w));
            Samp1DRealFunc fn = new(f: (w, p) => NReal(w));
            return fn.Sample(xs: w, loopMode: loopMode);
        }

        /// <summary>
        /// evaluates nReal = nReal(λ0, λ1, ... , λn) with λ 
        /// defined on a uniform sampling grid
        /// </summary>
        /// <param name="w"> uniform sampling grid of λ </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> real-parts of refractive indices </returns>
        [Obsolete]
        public VectorD EvaluateReal(GridInfo1D w,
            LoopMode loopMode = Defaults.LoopOption)
            => EvaluateReal((ScatInfo1D)w, loopMode);

        /// <summary>
        /// evaluates nImag = nImag(λ0, λ1, ... , λn) on a set of 
        /// wavelengths, either uniform or scattered
        /// </summary>
        /// <param name="w"> samples of wavelength </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> imaginary-parts of refractive indices </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [Obsolete]
        public VectorD EvaluateImag(ScatInfo1D w,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (w[0] < WavelengthMin || w[w.Count - 1] > WavelengthMax)
                throw new ArgumentOutOfRangeException(nameof(w));
            Samp1DRealFunc fn = new(f: (w, p) => NImag(w));
            return fn.Sample(xs: w, loopMode: loopMode);
        }

        /// <summary>
        /// evaluates nImag = nImag(λ0, λ1, ... , λn) with λ 
        /// defined on a uniform sampling grid
        /// </summary>
        /// <param name="w"> uniform sampling grid of λ </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> imaginary-parts of refractive indices </returns>
        [Obsolete]
        public VectorD EvaluateImag(GridInfo1D w,
            LoopMode loopMode = Defaults.LoopOption)
            => EvaluateImag((ScatInfo1D)w, loopMode);

        /// <summary>
        /// evaluates n = n(λ0, λ1, ... , λn) on a set of 
        /// wavelengths, either uniform or scattered
        /// </summary>
        /// <param name="w"> samples of wavelength </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> complex refractive indices </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [Obsolete]
        public VectorZ Evaluate(ScatInfo1D w, 
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (w[0] < WavelengthMin || w[w.Count - 1] > WavelengthMax) 
                throw new ArgumentOutOfRangeException(nameof(w));
            Samp1DCplxFunc fn = new(f: (w, p) => N(w));
            return fn.Sample(xs: w, loopMode: loopMode);
        }

        /// <summary>
        /// evaluates n = n(λ0, λ1, ... , λn) with λ 
        /// defined on a uniform sampling grid
        /// </summary>
        /// <param name="w"> uniform sampling grid of λ </param>
        /// <param name="loopMode"> loop-computation option </param>
        /// <returns> complex refractive indices </returns>
        [Obsolete]
        public VectorZ Evaluate(GridInfo1D w,
            LoopMode loopMode = Defaults.LoopOption)
            => Evaluate((ScatInfo1D)w, loopMode);

        #endregion
        #region ---- equals ----

        /// <summary>
        /// Determines whether the current <see cref="Material"/> instance is equal to another <see cref="Material"/> instance.
        /// </summary>
        /// <param name="other">The other <see cref="Material"/> instance to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Material"/> is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Material? other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            if (other is null) { return false; }

            // Compare value properties
            if (WavelengthMin != other.WavelengthMin
                || WavelengthMax != other.WavelengthMax) { return false; }

            // Compare delegates by reference equality (best-effort, as delegates may be closures)
            if (!Equals(NReal, other.NReal) ||
                !Equals(NImag, other.NImag) ||
                !Equals(N, other.N) ||
                !Equals(Epsilon, other.Epsilon) ||
                !Equals(Mu, other.Mu))
            { return false; }

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="Material"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="Material"/> instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is a <see cref="Material"/> and is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Material);
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="Material"/> instance.
        /// </summary>
        /// <remarks>
        /// The hash code is computed by combining the hash codes of the material's wavelength range and its property delegates.
        /// This provides a best-effort hash for use in hash-based collections. Note that delegate hash codes are not guaranteed to be unique for functionally equivalent delegates.
        /// </remarks>
        /// <returns>
        /// An integer hash code representing the current <see cref="Material"/> instance.
        /// </returns>
        public override int GetHashCode()
        {
            // Use a simple hash code combining the properties
            return HashCode.Combine(WavelengthMin, WavelengthMax, NReal, NImag, N, Epsilon, Mu);
        }

        #endregion

        #endregion

    }
}
