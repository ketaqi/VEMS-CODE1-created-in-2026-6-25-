using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VEMS.MathCore;
using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Represents a two-dimensional (2D) layer medium with
    /// piecewise-constant permittivity and permeability value.
    /// </summary>
    public class Layer2DPwctMedium : ILayerMedium2D
    {
        #region properties

        /// <summary>
        /// Piecewise-constant complex permittivity data for the 2D medium.
        /// </summary>
        public Pwct2DCplxData EpsilonData { get; set; }

        /// <summary>
        /// Piecewise-constant complex permeability data for the 2D medium.
        /// </summary>
        public Pwct2DCplxData? MuData { get; set; }

        /// <summary>
        /// Gets the permittivity distribution function.
        /// epsilon = f(λ, y, x)
        /// </summary>
        public Func<double, double, double, Complex> Epsilon
        {
            get => (lambda, y, x) => EpsilonData.Evaluate(x, y);
        }

        /// <summary>
        /// Gets the permeability distribution function.
        /// mu = f(λ, y, x)
        /// </summary>
        public Func<double, double, double, Complex>? Mu
        {
            get => MuData == null ? null : (lambda, y, x) => MuData.Evaluate(x, y);
        }

        #region cache info

        public bool CacheSampleData { get; set; }

        public MatrixZ? SampleData { get; set; }

        public GridInfo2D? SampGrid { get; set; }

        public MaterialProperty SelectedProp { get; set; }

        #endregion

        #endregion

        #region constructors

        public Layer2DPwctMedium(
            Pwct2DCplxData epsilon,
            Pwct2DCplxData? mu = null)
        {
            EpsilonData = epsilon;
            MuData = mu;
        }

        #endregion

        #region methods

        /// <summary>
        /// Samples epsilon / mu on a uniform 2D grid.
        /// </summary>
        public (MatrixZ epsilon, MatrixZ? mu) SampleMedium(
            double wavelength,
            GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = true)
        {
            MatrixZ eps = EpsilonData.SampleOnGrid(grid, loopMode);
            MatrixZ? mu = MuData == null ? null : MuData.SampleOnGrid(grid, loopMode);

            if (cacheSampleData)
            {
                CacheSampleData = true;
                SampleData = eps;
                SampGrid = new GridInfo2D(grid);
                SelectedProp = MaterialProperty.Epsilon;
            }
            else
            {
                CacheSampleData = false;
            }

            return (eps, mu);
        }

        /// <summary>
        /// Returns the permittivity sampled in the k-domain using the direct rule.
        /// </summary>
        public (MatrixZ epsilonK, MatrixZ? muK) MediumInDirKdomain(
            double wavelength,
            GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            long startIndexY = -(grid.Rows - 1) / 2;
            long startIndexX = -(grid.Cols - 1) / 2;
            Debug.WriteLine(startIndexY);
            Debug.WriteLine(startIndexX);
            MatrixZ epsK = Transform.ForwardTransform2D(
                x: EpsilonData,
                startIndexY: startIndexY,
                numCoeffY: grid.Rows,
                startIndexX: startIndexX,
                numCoeffX: grid.Cols,
                loopMode: loopMode
            );

            MatrixZ? muK = null;
            if (MuData != null)
            {
                muK = Transform.ForwardTransform2D(
                    x: MuData,
                    startIndexY: startIndexY,
                    numCoeffY: grid.Rows,
                    startIndexX: startIndexX,
                    numCoeffX: grid.Cols,
                    loopMode: loopMode
                );
            }

            return (epsK, muK);
        }

        /// <summary>
        /// Returns the permittivity sampled in the k-domain using the inverse rule.
        /// </summary>
        public (MatrixZ epsilonK, MatrixZ? muK) MediumInInvKdomain(
            double wavelength,
            GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption,
            bool cacheSampleData = false)
        {
            long startIndexY = -(grid.Rows - 1) / 2;
            long startIndexX = -(grid.Cols - 1) / 2;

            Pwct2DCplxData epsInv = InvertPwctData(EpsilonData);
            MatrixZ epsK = Transform.ForwardTransform2D(
                x: epsInv,
                startIndexY: startIndexY,
                numCoeffY: grid.Rows,
                startIndexX: startIndexX,
                numCoeffX: grid.Cols,
                loopMode: loopMode
            );

            MatrixZ? muK = null;
            if (MuData != null)
            {
                Pwct2DCplxData muInv = InvertPwctData(MuData);
                muK = Transform.ForwardTransform2D(
                    x: muInv,
                    startIndexY: startIndexY,
                    numCoeffY: grid.Rows,
                    startIndexX: startIndexX,
                    numCoeffX: grid.Cols,
                    loopMode: loopMode
                );
            }

            return (epsK, muK);
        }

        #endregion

        #region helpers

        private static Pwct2DCplxData InvertPwctData(Pwct2DCplxData src)
        {
            var regions = new List<Pwct2DCplxData.Region2D>();

            foreach (var r in src.Regions)
            {
                regions.Add(new Pwct2DCplxData.Region2D(
                    shape: r.Shape,
                    value: Complex.One / r.Value,
                    centerX: r.CenterX,
                    centerY: r.CenterY,
                    a: r.A,
                    b: r.B,
                    angle: r.Angle
                ));
            }

            return new Pwct2DCplxData(
                periodX: src.PeriodX,
                periodY: src.PeriodY,
                backgroundValue: Complex.One / src.BackgroundValue,
                regions: regions
            );
        }

        #endregion
    }
}
