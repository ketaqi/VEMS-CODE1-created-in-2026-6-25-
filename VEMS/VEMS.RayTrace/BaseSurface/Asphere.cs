using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.RayTrace
{
    /// <summary>
    /// aspherical surface
    /// </summary>
    public class Asphere
    {
        #region properties

        // center, coordinate ...

        /// <summary>
        /// radius of the asphere & coefficients
        /// </summary>
        public double Radius { get; set; }
        public double K { get; set; }
        public double[] A { get; set; } 

        private int N = 9;//Control the maximum number of coefficients

        #endregion
        #region constructor

        /// <summary>
        /// constructs an aspherical surface
        /// with default 100 mm radius of curvature
        /// with all default coefficients to be 0
        /// </summary>
        public Asphere()
        {
            Radius = 100E-3; // 100.0 mm
            K = 0;
            A = new double[N];
        }

        /// <summary>
        /// constructs a spherical surface 
        /// with given VectorD of coefficients
        /// </summary>
        /// <param name="radius"> radius of curvature </param>
        public Asphere(double radius, double k, VectorD coefficients)
        {            
            if (coefficients.Count > N)
            {
                throw new Exception("The maximum order limit is exceeded");
            }
            Radius = radius;
            K = k;
            A = new double[N];
            for (int i = 0; i < coefficients.Count; i++)
            {
                A[i] = coefficients[i];
            }
        }

        #endregion
        #region methods

        /// <summary>
        /// intersect with incoming ray
        /// </summary>
        /// <param name="rIn"> incoming ray vector position </param>
        /// <param name="sIn"> incoming ray vector direction [unit vector] </param>
        /// <param name="rOut"> [out] intersection position on the conic </param>
        /// <param name="p"> [out] distance from incoming position to intersection </param>
        /// <returns> if intersection is found, return true; else, false </returns>
        public bool Intersect(VecD3 rIn, VecD3 sIn,
            out VecD3? rOut,
            out double p)
        {
            // initialize output
            rOut = null;
            p = 0.0;

            // auxiliary variables
            double rho = 1.0 / Radius;
            double l1 = - VecD3.Dot(sIn, rIn);
            VecD3 m0 = new()
            {
                X = rIn.X + l1 * sIn.X,
                Y = rIn.Y + l1 * sIn.Y,
                Z = rIn.Z + l1 * sIn.Z
            };
            double m2 = VecD3.NormSquare(m0);
            double delta = sIn.Z * sIn.Z - m2 * rho * rho + 2.0 * m0.Z * rho;

            // check if there is at least one intersection
            if (delta < 0.0) { return false; }

            // continue if delta >= 0.0
            if (Math.Abs(sIn.Z) <= 1e-15) { return false; }
            // The direction vector has to have a z component方向向量z分量必须不为0
            
            double t;
            //t depends on r plus or minus and the direction of the ray 取哪个t由r的正负和光线方向决定
            
            if (sIn.Z * Radius > 0.0)
            {
                t = (m2 * rho - 2.0 * m0.Z) / (sIn.Z + Math.Sqrt(delta));
            }
            else 
            {
                t = (m2 * rho - 2.0 * m0.Z) / (sIn.Z - Math.Sqrt(delta));
            }
            p = l1 + t;

            VecD3 E01 = new()
            {
                X = rIn.X + sIn.X * p,
                Y = rIn.Y + sIn.Y * p,
                Z = rIn.Z + sIn.Z * p
            };
            double x1=E01.X;
            double y1=E01.Y;
            double z1=E01.Z;
            double z_;
            do
            {
                double H = Math.Sqrt(x1 * x1 + y1 * y1);
                z_ = rho * H * H / (1 + Math.Sqrt(1 - (1 + K) * rho * rho * H * H)) + A[0] * Math.Pow(H, 4.0) + A[1] * Math.Pow(H, 6.0) + A[2] * Math.Pow(H, 8.0) + A[3] * Math.Pow(H, 10.0) + A[4] * Math.Pow(H, 12.0) + A[5] * Math.Pow(H, 14.0) + A[6] * Math.Pow(H, 16.0) + A[7] * Math.Pow(H, 18.0) + A[8] * Math.Pow(H, 20.0);
                double l = x1 * (rho / Math.Sqrt(1 - (1 + K) * rho * rho * H * H) + 4 * A[0] * Math.Pow(H, 2.0) + 6 * A[1] * Math.Pow(H, 4.0) + 8 * A[2] * Math.Pow(H, 6.0) + 10 * A[3] * Math.Pow(H, 8.0) + 12 * A[4] * Math.Pow(H, 10.0) + 14 * A[5] * Math.Pow(H, 12.0) + 16 * A[6] * Math.Pow(H, 14.0) + 18 * A[7] * Math.Pow(H, 16.0) + 20 * A[8] * Math.Pow(H, 18.0));
                double m = y1 * (rho / Math.Sqrt(1 - (1 + K) * rho * rho * H * H) + 4 * A[0] * Math.Pow(H, 2.0) + 6 * A[1] * Math.Pow(H, 4.0) + 8 * A[2] * Math.Pow(H, 6.0) + 10 * A[3] * Math.Pow(H, 8.0) + 12 * A[4] * Math.Pow(H, 10.0) + 14 * A[5] * Math.Pow(H, 12.0) + 16 * A[6] * Math.Pow(H, 14.0) + 18 * A[7] * Math.Pow(H, 16.0) + 20 * A[8] * Math.Pow(H, 18.0));
                double l2 = (z_ - z1) / (-sIn.X * l - sIn.Y * m + sIn.Z);
                x1 += sIn.X * l2;
                y1 += sIn.Y * l2;
                z1 += sIn.Z * l2;
            }
            while (Math.Abs(z1 - z_) > 1e-18);
            rOut = new()
            {
                X = x1,
                Y = y1,
                Z = z1,
            };
            return true;
        }

        /// <summary>
        /// computes surface normal at 
        /// a given point on the sphere
        /// </summary>
        /// <param name="intersection"> intersection on the sphere</param>
        /// <returns> surface normal vector </returns>
        public VecD3 ComputeNormal(VecD3 intersection)
        {

            double H = Math.Sqrt(intersection.X * intersection.X + intersection.Y * intersection.Y);
            double rho = 1.0 / Radius;
            double l = intersection.X * (rho / Math.Sqrt(1 - (1 + K) * rho * rho * H * H) + 4 * A[0] * Math.Pow(H, 2.0) + 6 * A[1] * Math.Pow(H, 4.0) + 8 * A[2] * Math.Pow(H, 6.0) + 10 * A[3] * Math.Pow(H, 8.0) + 12 * A[4] * Math.Pow(H, 10.0) + 14 * A[5] * Math.Pow(H, 12.0) + 16 * A[6] * Math.Pow(H, 14.0) + 18 * A[7] * Math.Pow(H, 16.0) + 20 * A[8] * Math.Pow(H, 18.0));
            double m = intersection.Y * (rho / Math.Sqrt(1 - (1 + K) * rho * rho * H * H) + 4 * A[0] * Math.Pow(H, 2.0) + 6 * A[1] * Math.Pow(H, 4.0) + 8 * A[2] * Math.Pow(H, 6.0) + 10 * A[3] * Math.Pow(H, 8.0) + 12 * A[4] * Math.Pow(H, 10.0) + 14 * A[5] * Math.Pow(H, 12.0) + 16 * A[6] * Math.Pow(H, 14.0) + 18 * A[7] * Math.Pow(H, 16.0) + 20 * A[8] * Math.Pow(H, 18.0));
            double n = -1;
            double l1 = -l / Math.Sqrt(l * l + m * m + n * n);
            double m1 = -m / Math.Sqrt(l * l + m * m + n * n);
            double n1 = -n / Math.Sqrt(l * l + m * m + n * n);
            return new VecD3
            {
                X = l1,
                Y = m1,
                Z = n1,
            };
        }

        /// <summary>
        /// computes refracted ray direction
        /// after intersection is found and 
        /// surface normal is computed
        /// </summary>
        /// <param name="sIn"> incoming ray direction [unit vector] </param>
        /// <param name="n"> spherical surface normal at intersection </param>
        /// <param name="nFront"> refractive index in front </param>
        /// <param name="nBehind"> refractive index behind </param>
        /// <returns> refracted ray direction </returns>
        public static VecD3 Refract(VecD3 sIn, VecD3 n,
            double nFront, double nBehind)
        {
            double cosI = VecD3.Dot(sIn, n);
            double rn = nFront / nBehind;
            double cosIP = Math.Sqrt(1.0 - Math.Pow(rn, 2.0) * (1.0 - cosI * cosI));
            double tau = nBehind * cosIP - nFront * cosI;
            double rtau = tau / nBehind;

            return rn * sIn + rtau * n;
        }

        /// <summary>
        /// computes reflected ray direction
        /// after intersection is found and 
        /// surface normal is computed ...
        /// </summary>
        /// <param name="sIn"> incoming ray direction </param>
        /// <param name="n"> spherical surface normal at intersection </param>
        /// <returns> reflected ray direction </returns>
        public static VecD3 Reflect(VecD3 sIn, VecD3 n)
        {
            double cosI = VecD3.Dot(sIn, n);
            return sIn - 2.0 * cosI * n;
        }

        #endregion


    }
}
