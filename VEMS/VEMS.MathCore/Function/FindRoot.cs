
namespace VEMS.MathCore
{
    /// <summary>
    /// root-finding methods
    /// </summary>
    public class FindRoot
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="parameters"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        [Obsolete]
        public static double BisectionPara(Func<double, List<double>?, double> f, 
            List<double>? parameters, double a, double b, 
            double tolerance)
        {
            double mid = a;
            while ((b - a) / 2 > tolerance)
            {
                mid = (a + b) / 2;
                if (f(mid, parameters) == 0)
                    return mid;
                else if (f(a, parameters) * f(mid, parameters) < 0)
                    b = mid;
                else
                    a = mid;
            }
            return mid;
        }

        [Obsolete]
        public static double BisectionSimp(Func<double, double> f,
            double a, double b,
            double tolerance)
        {
            double mid = a;
            while ((b - a) / 2 > tolerance)
            {
                mid = (a + b) / 2;
                if (f(mid) == 0)
                    return mid;
                else if (f(a) * f(mid) < 0)
                    b = mid;
                else
                    a = mid;
            }
            return mid;
        }

        #region ---- Bisection ----

        /// <summary> 
        /// finds a solution of the equation f(x)=0
        /// using bisection method
        /// </summary>
        /// <param name="f"> the function to find roots from </param>
        /// <param name="lowerBound"> the lower bound of the range where the root is supposed to be </param>
        /// <param name="upperBound"> the upper bound of the range where the root is supposed to be </param>
        /// <param name="accuracy"> desired accuracy: the root will be refined until the accuracy or the maximum number of iterations is reached </param>
        /// <param name="maxIterations"> maximum number of iterations </param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        public static double Bisection(Func<double, double> f,
            double lowerBound, double upperBound,
            double accuracy = 1e-14,
            long maxIterations = 100)
        {
            if (TryBisection(f, lowerBound, upperBound, accuracy, maxIterations, out double root))
            { return root; }
            throw new ArithmeticException($"The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
        }

        /// <summary>
        /// try to find a solution of the equation f(x)=0 
        /// using bisection method 
        /// </summary>
        /// <param name="f"> the function to find roots from </param>
        /// <param name="lowerBound"> the lower bound of the range where the root is supposed to be </param>
        /// <param name="upperBound"> the upper bound of the range where the root is supposed to be </param>
        /// <param name="accuracy"> desired accuracy: the root will be refined until the accuracy or the maximum number of iterations is reached </param>
        /// <param name="maxIterations"> maximum number of iterations </param>
        /// <param name="root"> the root that was found, if any; undefined if the function returns false </param>
        /// <returns> true if a root with the specified accuracy was found, else false </returns>
        internal static bool TryBisection(Func<double, double> f,
            double lowerBound, double upperBound,
            double accuracy, long maxIterations,
            out double root)
        {
            if (accuracy <= 0)
            { throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero."); }
            if (upperBound < lowerBound)
            { (upperBound, lowerBound) = (lowerBound, upperBound); }

            double fMin = f(lowerBound);
            if (Math.Sign(fMin) == 0)
            {
                root = lowerBound;
                return true;
            }

            double fMax = f(upperBound);
            if (Math.Sign(fMax) == 0)
            {
                root = upperBound;
                return true;
            }

            root = 0.5 * (lowerBound + upperBound);

            // bad bracketing?
            if (Math.Sign(fMin) == Math.Sign(fMax))
            { return false; }

            for (long i = 0; i <= maxIterations; i++)
            {
                double fRoot = f(root);
                if (upperBound - lowerBound <= 2 * accuracy && Math.Abs(fRoot) <= accuracy)
                { return true; }

                if ((lowerBound == root) || (upperBound == root))
                { return false; }

                if (Math.Sign(fRoot) == Math.Sign(fMin))
                {
                    lowerBound = root;
                    fMin = fRoot;
                }
                else if (Math.Sign(fRoot) == Math.Sign(fMax))
                {
                    upperBound = root;
                    fMax = fRoot;
                }
                else // Math.Sign(froot) == 0
                { return true; }

                root = 0.5 * (lowerBound + upperBound);
            }

            return false;
        }

        #endregion

    }


    internal class NewtonMethod
    {
        #region  properties
        /// <summary>
        /// The difference between two functions
        /// </summary>
        public Func<double, List<double>?, double> F { get; set; }
        /// <summary>
        /// The derivative of the difference between two functions
        /// </summary>
        public Func<double, List<double>?, double> FPrime { get; set; }
        #endregion
        #region  Constructors
        public NewtonMethod(Func<double, List<double>?, double> h, Func<double, List<double>?, double> hPrime)
        {
            F = h;
            FPrime = hPrime;
        }
        #endregion
        #region  Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">The difference between two functions</param>
        /// <param name="hPrime">The derivative of the difference between two functions</param>
        /// <param name="x0">Initial guess</param>
        /// <param name="tolerance">tolerance</param>
        /// <param name="maxIterations">maximum number of iterations</param>
        /// <returns></returns>
        public static double Newton(Func<double, List<double>?, double> h, Func<double, List<double>?, double> hPrime, double x0, double tolerance, int maxIterations)
        {
            double x = x0;
            for (int i = 0; i < maxIterations; i++)
            {
                double hValue = h(x, null);
                double hPrimeValue = hPrime(x, null);
                if (Math.Abs(hValue) < tolerance)
                {
                    return x;
                }
                x = x - hValue / hPrimeValue;
            }
            throw new Exception("Newton's method did not converge");
        }


        #endregion
    }




}
