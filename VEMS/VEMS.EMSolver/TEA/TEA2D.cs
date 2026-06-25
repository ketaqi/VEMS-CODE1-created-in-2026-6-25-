using System.Numerics;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Provides helper methods for 2D Thin-Element-Approximation (TEA).
    /// Includes phase quantization, phase wrapping, and single-layer transmission computation.
    /// </summary>
    public static class TEA2D
    {
        #region helpers

        /// <summary>
        /// Quantizes a phase function to discrete levels, optionally enforcing the result to be within [-π, π).
        /// </summary>
        /// <param name="phi">The input phase function φ(x, y).</param>
        /// <param name="enforce2Pi">If true, clamps the quantized phase to the range [-π, π); otherwise, allows unbounded quantization.</param>
        /// <param name="nLevel">The number of quantization levels.</param>
        /// <param name="offset">An optional offset to add to the quantized phase.</param>
        /// <returns>
        /// A function that returns the quantized phase at (x, y).
        /// </returns>
        public static Func<double, double, double> QuantizePhase(
            Func<double, double, double> phi,
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
                return (x, y) =>
                {
                    double val = phi(x, y) + pi;
                    long m = (long)Math.Floor(val * invDPhi);
                    m = m < 0 ? 0 : (m >= nLevel ? nLevel - 1 : m);
                    return m * dPhi - pi + offset;
                };
            }
            else
            {
                // No clamping, slightly faster
                return (x, y) =>
                {
                    double val = phi(x, y) + pi;
                    long m = (long)Math.Floor(val * invDPhi);
                    return m * dPhi - pi + offset;
                };
            }
        }

        /// <summary>
        /// Wraps a phase function to the interval [-π, π).
        /// </summary>
        /// <param name="phi">The input phase function φ(x, y).</param>
        /// <param name="offset">An optional offset to add to the wrapped phase.</param>
        /// <returns>
        /// A function that returns the wrapped phase at (x, y).
        /// </returns>
        public static Func<double, double, double> WrapPhase(
            Func<double, double, double> phi,
            double offset = 0.0)
        {
            const double twoPi = 2.0 * Math.PI;
            return (x, y) =>
            {
                double phase = phi(x, y);
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
        /// Computes the transmission function for a single 2D layer using the Thin-Element-Approximation (TEA).
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="Medium2D"/> representing the 2D medium layer.</typeparam>
        /// <param name="layer">The 2D medium layer for which the transmission is computed.</param>
        /// <param name="wavelength">The wavelength in vacuum (λ).</param>
        /// <param name="thickness">The thickness of the layer.</param>
        /// <param name="n0">
        /// The reference refractive index. If not specified or set to <c>default</c>, the absolute refractive index of the layer is used.
        /// If specified, the computation uses the difference <c>layer.N - n0</c>.
        /// </param>
        /// <param name="isPhaseOnly">
        /// If <c>true</c>, only the phase function is computed and the amplitude is omitted.
        /// If <c>false</c>, both phase and amplitude functions are computed.
        /// </param>
        /// <returns>
        /// A <see cref="Transmission2D"/> object containing the computed phase (and amplitude, if requested) functions.
        /// </returns>
        public static Transmission2D Compute<T>(T layer,
            double wavelength, double thickness,
            Complex n0 = default,
            bool isPhaseOnly = false)
            where T : Medium2D
        {
            // Precompute constants and cache delegate
            double k0 = 2.0 * Math.PI / wavelength;
            var n = n0 == default ? layer.N : (w, x, y) => layer.N(w, x, y) - n0;
            double nRe(double x, double y) => n(wavelength, x, y).Real;
            Transmission2D t = new()
            {
                Phase = (x, y) => k0 * nRe(x, y) * thickness
            };
            if (!isPhaseOnly)
            {
                double nIm(double x, double y) => n(wavelength, x, y).Imaginary;
                t.Amplitude = (x, y) => Math.Exp( -k0 * nIm(x, y) * thickness);
            }
            return t;
        }

        #endregion
    }


}
