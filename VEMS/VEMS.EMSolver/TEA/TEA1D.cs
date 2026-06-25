using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Provides methods for 1D Thin-Element-Approximation (TEA).
    /// Includes phase quantization, phase wrapping, and single-layer transmission computation.
    /// </summary>
    public static class TEA1D
    {
        #region helpers 

        /// <summary>
        /// Quantizes the input phase function to a specified number of discrete levels.
        /// Optionally enforces the phase to be within the range [-π, π).
        /// </summary>
        /// <param name="phi">Input phase function to quantize.</param>
        /// <param name="enforce2Pi">If true, clamps the quantized phase to the range [-π, π); otherwise, allows unbounded quantization.</param>
        /// <param name="nLevel">Number of phase quantization levels.</param>
        /// <param name="offset">Constant offset added to the quantized phase result.</param>
        /// <returns>A function representing the quantized phase.</returns>
        public static Func<double, double> QuantizePhase(Func<double, double> phi,
            bool enforce2Pi = true,
            long nLevel = 2, double offset = 0.0)
        {
            // Precompute constants for efficiency
            double dPhi = 2.0 * Math.PI / nLevel;
            double invDPhi = 1.0 / dPhi;
            const double pi = Math.PI;

            if (enforce2Pi)
            {
                // Clamp m to [0, nLevel-1] only if needed
                return x =>
                {
                    double val = phi(x) + pi;
                    long m = (long)Math.Floor(val * invDPhi);
                    m = m < 0 ? 0 : (m >= nLevel ? nLevel - 1 : m);
                    return m * dPhi - pi + offset;
                };
            }
            else
            {
                // No clamping, slightly faster
                return x =>
                {
                    double val = phi(x) + pi;
                    long m = (long)Math.Floor(val * invDPhi);
                    return m * dPhi - pi + offset;
                };
            }
        }

        /// <summary>
        /// Wraps the input phase function to the range [-π, π).
        /// </summary>
        /// <param name="phi">Input phase function to wrap.</param>
        /// <param name="offset">Constant offset added to the wrapped phase result.</param>
        /// <returns>A function representing the wrapped phase.</returns>
        public static Func<double, double> WrapPhase(Func<double, double> phi,
            double offset = 0.0)
        {
            const double twoPi = 2.0 * Math.PI;
            return x =>
            {
                double phase = phi(x);
                // Use Math.IEEERemainder for efficient wrapping to [-π, π)
                double wrapped = Math.IEEERemainder(phase, twoPi);
                // Adjust for the exclusive upper bound at π
                if (wrapped < -Math.PI)
                    wrapped += twoPi;
                else if (wrapped >= Math.PI)
                    wrapped -= twoPi;
                return wrapped + offset;
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// Computes the 1D transmission function for a single layer medium.
        /// Returns a <see cref="Transmission1D"/> object with phase (and optionally amplitude) functions.
        /// </summary>
        /// <typeparam name="T">Type of the medium, must inherit from <see cref="Medium1D"/>.</typeparam>
        /// <param name="layer">The 1D medium layer.</param>
        /// <param name="wavelength">Wavelength in vacuum.</param>
        /// <param name="thickness">Thickness of the layer.</param>
        /// <param name="n0">
        /// Optional reference refractive index. If not specified or set to <c>default</c>, the layer's refractive index is used directly.
        /// If specified, the refractive index used is <c>layer.N(wavelength, x) - n0</c>.
        /// </param>
        /// <param name="isPhaseOnly">
        /// If <c>true</c>, only the phase function is computed and the amplitude is not set.
        /// If <c>false</c>, both phase and amplitude functions are computed.
        /// </param>
        /// <returns>
        /// A <see cref="Transmission1D"/> object representing the transmission function, with phase (and optionally amplitude) functions set.
        /// </returns>
        public static Transmission1D Compute<T>(T layer,
            double wavelength, double thickness,
            Complex n0 = default,
            bool isPhaseOnly = false)
            where T : Medium1D
        {
            // Precompute constants and cache delegate
            double k0 = 2.0 * Math.PI / wavelength;
            var n = n0 == default ? layer.N : (w, x) => layer.N(w, x) - n0;
            double nRe(double x) => n(wavelength, x).Real;
            Transmission1D t = new()
            {
                Phase = x => k0 * nRe(x) * thickness
            };
            if (!isPhaseOnly)
            {
                double nIm(double x) => n(wavelength, x).Imaginary;
                t.Amplitude = x => Math.Exp( -k0 * nIm(x) * thickness);
            }
            return t;
        }

        #endregion
    }

}
