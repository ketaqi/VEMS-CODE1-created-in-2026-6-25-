using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    public interface ILayerMedium2D
    {
        #region properties

        /// <summary>
        /// Gets the permittivity (epsilon) of the 2D layer medium.
        /// epsilon = f(λ, y, x)
        /// </summary>
        Func<double, double, double, Complex> Epsilon { get; }

        /// <summary>
        /// Gets the permeability (mu) of the 2D layer medium. Can be null.
        /// mu = f(λ, y, x)
        /// </summary>
        Func<double, double, double, Complex>? Mu { get; }

        #endregion

        #region methods

        /// <summary>
        /// Samples the specified material property of the medium on a target uniform 2D grid.
        /// </summary>
        (MatrixZ epsilon, MatrixZ? mu) SampleMedium(
            double wavelength,
            GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = true);

        /// <summary>
        /// Samples the permittivity in the k-domain (Fourier domain) using the direct rule.
        /// </summary>
        (MatrixZ epsilonK, MatrixZ? muK) MediumInDirKdomain(
            double wavelength,
            GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false);

        /// <summary>
        /// Samples the permittivity in the k-domain (Fourier domain) using the inverse rule.
        /// </summary>
        (MatrixZ epsilonK, MatrixZ? muK) MediumInInvKdomain(
            double wavelength,
            GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false);

        #endregion
    }
}
