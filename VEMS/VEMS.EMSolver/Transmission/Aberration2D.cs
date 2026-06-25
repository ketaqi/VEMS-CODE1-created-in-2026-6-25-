
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// two-dimensional aberration function
    /// => phase-only transmission
    /// </summary>
    public class Aberration2D : Transmission2D
    {
        #region properties

        // ...

        #endregion
        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        public Aberration2D() { }

        /// <summary>
        /// constructs a 2D aberration function with given parameters
        /// </summary>
        /// <param name="shiftX"> lateral shift along x-direction </param>
        /// <param name="shiftY"> lateral shift along y-direction </param>
        /// <param name="scaling"> overall constant scaling factor </param>
        public Aberration2D(double shiftX = 0.0, double shiftY = 0.0,
            double scaling = 1.0)
        {
            ShiftX = shiftX;
            ShiftY = shiftY;
            Scaling = scaling;
        }

        #endregion
        #region methods

        // ...

        #endregion
        #region sub-classes

        /// <summary>
        /// 2D aberration defined by XY polynomial
        /// </summary>
        public class PolynomialXY : Aberration2D
        {
            #region constructor

            /// <summary>
            /// constructs a 2D aberration using XY polynomial
            /// </summary>
            /// <param name="px"></param>
            /// <param name="py"></param>
            /// <param name="shiftX"></param>
            /// <param name="shiftY"></param>
            /// <param name="scaling"></param>
            public PolynomialXY(List<double> px, List<double> py,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0)
                : base(shiftX, shiftY, scaling)
            {
                // creates XY polynomial in Cartesian coordinate system
                FunctionXY p = new(fx: Function1D.Polynomial, fy: Function1D.Polynomial);
                // defines phase function then
                Phase = (x, y) => Scaling * p.Evaluate(x, y, px, py);
            }

            #endregion
        }

        /// <summary>
        /// 2D aberration defined by Zernike polynomial
        /// </summary>
        public class Zernike : Aberration2D
        {
            #region constructor

            /// <summary>
            /// construcst a 2D aberration using Zernike polynomial
            /// </summary>
            /// <param name="idx"> single Zernike index </param>
            /// <param name="indexing"> Zernike indexing option </param>
            /// <param name="refRadius"> refernece radius </param>
            /// <param name="shiftX"> lateral shift along x-direction </param>
            /// <param name="shiftY"> lateral shift along y-direction </param>
            /// <param name="scaling"> overall constant scaling factor </param>
            public Zernike(int idx, CommonFunction.ZernikeIndexing indexing,
                double refRadius = 1.0,
                double shiftX = 0.0, double shiftY = 0.0,
                double scaling = 1.0)
                : base(shiftX, shiftY, scaling)
            {
                // creates Zernike in polar coordinate system first
                CommonFunction.Zernike z = new(idx, indexing, refRadius);
                // defines phase in Cartesian coordinate system
                Phase = (x, y) =>
                {
                    (double rho, double phi) = Converter.Cartesian2Polar(x, y);
                    return Scaling * z.Evaluate(rho, phi);
                };
            }

            /// <summary>
            /// construcst a 2D aberration using multiple Zernike polynomials
            /// </summary>
            /// <param name="indices"> list of Zernike indices </param>
            /// <param name="indexing"> Zernike indexing option </param>
            /// <param name="refRadius"> refernece radius </param>
            /// <param name="shiftX"> lateral shift along x-direction </param>
            /// <param name="shiftY"> lateral shift along y-direction </param>
            /// <param name="scalings"> list of scaling factors for each Zernike index </param>
            /// <exception cref="ArgumentException"></exception>
            public Zernike(List<int> indices,
                CommonFunction.ZernikeIndexing indexing,
                double refRadius = 1.0,
                double shiftX = 0.0, double shiftY = 0.0,
                List<double>? scalings = null)
                : base(shiftX, shiftY, 1.0)
            {
                // null and exception handling
                if (scalings == null)
                {
                    scalings = new List<double>(indices.Count);
                    for (int i = 0; i < indices.Count; i++) { scalings.Add(1.0); }
                }
                else
                {
                    if (indices.Count != scalings.Count)
                    { throw new ArgumentException("indices and scalings must have the same length"); }
                }

                Phase = (x, y) =>
                {
                    double result = 0.0;
                    (double rho, double phi) = Converter.Cartesian2Polar(x, y);
                    for (int i = 0; i < indices.Count; i++)
                    {
                        CommonFunction.Zernike zi = new(indices[i], indexing, refRadius);
                        result += scalings[i] * zi.Evaluate(rho, phi);
                    }
                    return result;
                };
            }

            #endregion
        }

        #endregion
    }

}
