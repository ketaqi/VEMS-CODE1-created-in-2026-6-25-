using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// set of parameters for LP mode
    /// <para> L: mode index l </para>
    /// <para> M: mode index m </para>
    /// <para> Beta: propagation constant </para>
    /// <para> C: mode coefficient </para>
    /// </summary>
    public struct LPParam
    {
        /// <summary>
        /// LP mode index L 
        /// starting from 0
        /// </summary>
        public int L { get; set; }

        /// <summary>
        /// LP mode index M
        /// starting from 1
        /// </summary>
        public int M { get; set; }

        /// <summary>
        /// propagation constant
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// complex LP mode coefficient
        /// </summary>
        public Complex C { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="l"> mode index l </param>
        /// <param name="m"> mode index m </param>
        /// <param name="beta"> propagation constant </param>
        /// <param name="c"> mode coefficient </param>
        public LPParam(int l, int m,
            double beta,
            double c = 1.0)
        {
            L = l;
            M = m;
            Beta = beta;
            C = c;
        }
    }


    /// <summary>
    /// LP mode class for cylindrical waveguide
    /// </summary>
    public class LPMode
    {
        #region properties

        /// <summary>
        /// wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// radius of the fiber's core
        /// </summary>
        public double CoreRadius { get; set; }

        /// <summary>
        /// permittivity of the core @Wavelength
        /// real-valued only
        /// </summary>
        public double EpsilonCore { get; set; }

        /// <summary>
        /// permittivity of the cladding @Wavelength
        /// real-valued only
        /// </summary>
        public double EpsilonCladding { get; set; }

        /// <summary>
        /// maximum number of LP modes
        /// </summary>
        public int MaxModeNumber { get; set; }

        /// <summary>
        /// list of mode triple parameters
        /// </summary>
        public List<LPParam> ModeParameters { get; } = [];

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        internal LPMode() { }

        /// <summary>
        /// construct a LP mode
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="coreRadius"> radius of fiber core </param>
        /// <param name="epsilonCore"> permittivity of fiber core (real-valued only) </param>
        /// <param name="epsilonCladding"> permittivity of cladding (real-valued only) </param>
        /// <param name="maxModeNumber"> limit of the number of LP modes for computation, default is 999 </param>
        public LPMode(double wavelength,
            double coreRadius,
            double epsilonCore,
            double epsilonCladding,
            int maxModeNumber = 999)
        {
            Wavelength = wavelength;
            CoreRadius = coreRadius;
            EpsilonCore = epsilonCore;
            EpsilonCladding = epsilonCladding;
            MaxModeNumber = maxModeNumber;
        }

        #endregion
        #region methods

        /// <summary>
        /// calculate a single LP mode with index l and m, if exists
        /// </summary>
        /// <param name="l"> desired mode index l </param>
        /// <param name="m"> desired mode index m </param>
        public void CalculateSingleMode(int l, int m)
        {
            // basic parameters
            double k0 = 2.0 * Math.PI / Wavelength; // vacuum wavenumber
            double v = k0 * CoreRadius * Math.Sqrt(EpsilonCore - EpsilonCladding); // normalized frequency 

            // define solution matrix determinant
            Func<double, double> solutionMatrixDet =
                delegate (double u)
                {
                    // auxiliary variable 
                    double w = Math.Sqrt(v * v - u * u);
                    // calculating matrix determinant using Bessel functions
                    double det;
                    if (l == 0)
                    {
                        det = SpecialFunctions.BesselJ(0, u) * w * SpecialFunctions.BesselK(1, w)
                            - SpecialFunctions.BesselK(0, w) * u * SpecialFunctions.BesselJ(1, u);
                        //det = Bessel.Jn(0, u, BesselSrcLib) * w * Bessel.Kn(1, w, BesselSrcLib)
                        //    - Bessel.Kn(0, w, BesselSrcLib) * u * Bessel.Jn(1, u, BesselSrcLib);
                    }
                    else
                    {
                        det = SpecialFunctions.BesselJ(l, u) * w * SpecialFunctions.BesselK(l - 1, w)
                            + SpecialFunctions.BesselK(l, w) * u * SpecialFunctions.BesselJ(l - 1, u);
                        //det = Bessel.Jn(l, u, BesselSrcLib) * w * Bessel.Kn(l - 1, w, BesselSrcLib)
                        //    + Bessel.Kn(l, w, BesselSrcLib) * u * Bessel.Jn(l - 1, u, BesselSrcLib);
                    }
                    return det;
                };

            // initialize beta - propagation constant
            double beta = 0.0;

            // initialize numerical root finder
            //!! FZero rootFinder = new FZero(); // from NMath

            // initialize root bracket
            double uMin = 0.0;
            double uMax = 0.0;

            // initialize JnZeroSolver
            //!! _besselZeroSolvers = new List<Bessel.JnZeroSolver>();
            if (l == 0)
            {
                //!! _besselZeroSolvers.Add(new Bessel.JnZeroSolver(0, BesselSrcLib, FindRootSrcLib));
                //!! _besselZeroSolvers.Add(new Bessel.JnZeroSolver(1, BesselSrcLib, FindRootSrcLib));

            }
            else
            {
                for (int i = 0; i <= l; i++)
                {
                    //!! _besselZeroSolvers.Add(new Bessel.JnZeroSolver(i, BesselSrcLib, FindRootSrcLib));
                }
            }


            // define values according to cases
            if (l == 0 && m == 1)
            {
                //!! uMax = Bessel.JnZero(0, 1, BesselSrcLib);
            }
            else if (l == 0)
            {
                //!! uMin = Bessel.JnZero(l + 1, m - 1, BesselSrcLib);
                //!! uMax = Bessel.JnZero(l, m, BesselSrcLib);
            }
            else
            {
                //!! uMin = Bessel.JnZero(l - 1, m, BesselSrcLib);
                //!! uMax = Bessel.JnZero(l, m, BesselSrcLib);
            }
            // consistency check
            if (uMin >= v)
            {
                //modeExists = false; 
                //Globals.DataDisplay.LogMessage("No mode solution can be found for given input l and m. [uMin beyond the v limit]");
            }
            else
            {
                if (uMax >= v) // only looking for solution smaller than v
                {
                    //!! uMax = v - (v - uMin) * Globals.Defaults.DeviationThreshold;
                }
                // check function values at uMin and uMax
                double fuMin = solutionMatrixDet(uMin);
                double fuMax = solutionMatrixDet(uMax);
                if (fuMin * fuMax >= 0.0)
                {
                    //modeExists = false;
                    // Globals.DataDisplay.LogMessage("No mode solution can be found for given input l and m. [no solution between uMin and uMax]");
                }
                else
                {
                    //modeExists = true;
                    // find root for solution matrix determinant
                    //!! double u = rootFinder.Find(new OneVariableFunction(solutionMatrixDet),
                    //!!    uMin, uMax);
                    //!! beta = Math.Sqrt(k0 * k0 * EpsilonCore - u * u / (CoreRadius * CoreRadius));
                }
            }

            // update
            ModeParameters.Add(new LPParam(l, m, beta));

        }



        #endregion
    }
}
