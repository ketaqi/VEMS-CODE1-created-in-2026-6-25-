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
    /// spherical surface
    /// </summary>
    public class Sphere
    {
        #region properties

        // center, coordinate ...

        /// <summary>
        /// radius of the sphere
        /// </summary>
        public double Radius { get; set; }




        #endregion
        #region constructor

        /// <summary>
        /// constructs a spherical surface
        /// with default 100 mm radius of curvature
        /// </summary>
        public Sphere()
        {
            Radius = 100E-3; // 100.0 mm
        }

        /// <summary>
        /// constructs a spherical surface 
        /// with given radius of curvature
        /// </summary>
        /// <param name="radius"> radius of curvature </param>
        public Sphere(double radius)
        {
            Radius = radius;
        }

        #endregion
        #region methods

        /// <summary>
        /// intersect with incoming ray
        /// </summary>
        /// <param name="rIn"> incoming ray vector position </param>
        /// <param name="sIn"> incoming ray vector direction [unit vector] </param>
        /// <param name="rOut"> [out] intersection position on the sphere </param>
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
            double delta = sIn.Z * sIn.Z - m2 * rho * rho + 2.0 * m.Z * rho;

            // check if there is at least one intersection
            if (delta < 0.0) { return false; }

            // continue if delta >= 0.0
            if (Math.Abs(sIn.Z) <= 1e-15) { return false; }
            // The direction vector has to have a z component 方向向量z分量必须不为0
            double t;
            //t depends on r plus or minus and the direction of the ray 取哪个t由r的正负和光线方向决定
            if (sIn.Z * Radius > 0.0)
            {
                t = (m2 * rho - 2.0 * m.Z) / (sIn.Z + Math.Sqrt(delta));
            }
            else 
            {
                t = (m2 * rho - 2.0 * m.Z) / (sIn.Z - Math.Sqrt(delta));
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
        /// a given point on the sphere
        /// </summary>
        /// <param name="intersection"> intersection on the sphere</param>
        /// <returns> surface normal vector </returns>
        public VecD3 ComputeNormal(VecD3 intersection)
            => new()
            {
                X = -intersection.X / Radius,
                Y = -intersection.Y / Radius,
                Z = 1.0 - intersection.Z / Radius
            };

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
