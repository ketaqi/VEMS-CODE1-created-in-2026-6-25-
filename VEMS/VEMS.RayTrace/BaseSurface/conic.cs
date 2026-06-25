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
    /// conic surface
    /// </summary>
    public class Conic
    {
        #region properties

        // center, coordinate ...

        /// <summary>
        /// radius of the sphere，eccentricity of conic surface
        /// </summary>
        public double Radius { get; set; }
        public double K { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a conic surface
        /// with default 100 mm radius of curvature
        /// with default 0 of eccentricity of conic surface, which is equivalent to a sphere
        /// </summary>
        public Conic()
        {
            Radius = 100E-3; // 100.0 mm
            K = 0;
        }

        /// <summary>
        /// constructs a spherical surface 
        /// with given radius of curvature, given k=-e^2
        /// </summary>
        /// <param name="radius"> radius of curvature </param>
        /// <param name="k"> k=-e^2 </param>
        public Conic(double radius,double k)
        {
            Radius = radius;
            K = k;           
        }

        #endregion
        #region methods

        /// <summary>
        /// intersect with incoming ray
        /// </summary>
        /// <param name="rIn"> incoming ray vector position </param>
        /// <param name="sIn"> incoming ray vector direction [unit vector] </param>+
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
            double l = - VecD3.Dot(sIn, rIn);
            VecD3 m = new()
            {
                X = rIn.X + l * sIn.X,
                Y = rIn.Y + l * sIn.Y,
                Z = rIn.Z + l * sIn.Z
            };

            double m2 = VecD3.NormSquare(m);
            //double delta = Math.Pow((sIn.Z + E2 * m.Z * sIn.Z * rho),2.0)
                //- (1.0 - E2* sIn.Z * sIn.Z) * (m2 * rho * rho - 2.0 * m.Z * rho - E2 * m.Z * m.Z * rho * rho);
            double delta = Math.Pow((sIn.Z -K * m.Z * sIn.Z * rho), 2.0)
                - (1.0 + K * sIn.Z * sIn.Z) * (m2 * rho * rho - 2.0 * m.Z * rho + K * m.Z * m.Z * rho * rho);
            // check if there is at least one intersection
            if (delta < 0.0) { return false; }

            // continue if delta >= 0.0
            if (Math.Abs(sIn.Z) <= 1e-15) { return false; }
            // The direction vector has to have a z component方向向量z分量必须不为0
            double t;
            //t depends on r plus or minus and the direction of the ray 取哪个t由r的正负和光线方向决定
            if (sIn.Z * Radius > 0.0)
            {
                t = (m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho + Math.Sqrt(delta)) < ((m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho - Math.Sqrt(delta))) ?
                (m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho + Math.Sqrt(delta)) : (m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho - Math.Sqrt(delta));
            }
            else
            {
                t = (m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho + Math.Sqrt(delta)) > ((m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho - Math.Sqrt(delta))) ?
                  (m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho + Math.Sqrt(delta)) : (m2 * rho - 2.0 * m.Z + K * m.Z * m.Z * rho) / (sIn.Z - K * m.Z * sIn.Z * rho - Math.Sqrt(delta));
            }


            p = l + t;

            rOut = new()
            {
                X = rIn.X + sIn.X * p,
                Y = rIn.Y + sIn.Y * p,
                Z = rIn.Z + sIn.Z * p
            };

            return true;
        }

        /// <summary>
        /// computes surface normal at 
        /// a given point on the conic
        /// </summary>
        /// <param name="intersection"> intersection on the sphere</param>
        /// <returns> surface normal vector </returns>
        public VecD3 ComputeNormal(VecD3 intersection)
        {
            double k = 1.0 + K;
            double rho = 1.0 / Radius;
            double A = Math.Sqrt(-K * (intersection.X * intersection.X + intersection.Y * intersection.Y) * rho * rho + 1.0);
            return new VecD3
            {
                X = -intersection.X * rho / A,
                Y = -intersection.Y * rho / A,
                Z = (1.0 - intersection.Z * k * rho) / A              
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
