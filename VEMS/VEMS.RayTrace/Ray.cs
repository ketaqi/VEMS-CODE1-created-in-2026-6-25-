using System;
using System.Security.Cryptography.X509Certificates;
using VEMS.MathCore;
//using VEMS.EMSolver;
using Complex = System.Numerics.Complex;


namespace VEMS.RayTrace
{

    /// <summary>
    /// ray base class
    /// </summary>
    public class RayBase
    {
        #region properties

        /// <summary>
        /// 3D position vector (real-valued)
        /// </summary>
        public VecD3 Position{ get; set; }

        /// <summary>
        /// 3D direction vector (real-valued)
        /// </summary>
        public VecD3 Direction { get; set; }

        /// <summary>
        /// optical path length
        /// </summary>
        public double OPL { get; set; }

        /// <summary>
        /// wavelength in vacuum
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// wave number in vacuum
        /// </summary>
        public double K0 => 2.0 * Math.PI / Wavelength;
        
        /// <summary>
        /// complex refractive index 
        /// at the given wavelength
        /// </summary>
        public Complex RefractiveIndex { get; set; }


        /// <summary>
        /// transverse spatial frequency kx
        /// [simplified bi-direction conversion relation]
        /// </summary>
        public double Kx => K0 * RefractiveIndex.Real * Direction.X;

        /// <summary>
        /// transverse spatial frequency ky
        /// [sinplified bi-direction conversion relation]
        /// </summary>
        public double Ky => K0 * RefractiveIndex.Real * Direction.Y;

        #endregion
        #region constructors

        /// <summary>
        /// constructs a RayBase, with
        /// given wavelength, refractive index,
        /// default position, and direction
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="refractiveIndex"> complex refractive index </param>
        /// <param name="position"> 3D position of the ray </param>
        /// <param name="direction"> 3D direction of the ray </param>
        public RayBase(double wavelength, 
            Complex refractiveIndex,
            VecD3? position = null, 
            VecD3? direction = null)
        {
            Wavelength = wavelength;
            RefractiveIndex = refractiveIndex;
            Position = position?? VecD3.Zeros;
            Direction = direction?? VecD3.UnitZ;
        }

        #endregion
        #region methods

        //public void Propagate() { }
        //public void Intersect() { }




        #endregion
        #region static methods

        // intersect with spherical surface
        public static bool Intersect(VecD3 rIn, VecD3 sIn,
            double d, double r, 
            out VecD3 rOut,
            out double p)
        {
            // initialize output
            rOut = new VecD3();
            p = 0.0;

            // auxiliary variables
            double rho = 1.0 / r;
            double l = sIn.Z * d - VecD3.Dot(sIn, rIn);
            VecD3 m = new()
            {
                X = rIn.X + l * sIn.X,
                Y = rIn.Y + l * sIn.Y,
                Z = rIn.Z + l * sIn.Z - d
            };

            double m2 = VecD3.NormSquare(m);
            double delta = sIn.Z * sIn.Z - m2 * rho * rho + 2.0 * m.Z * rho;
            
            // check if there is at least one intersection
            if(delta < 0.0) { return false; }
            
            // continue if delta >= 0.0
            double t = (m2 * rho - 2.0 * m.Z) / (sIn.Z + Math.Sqrt(delta));
            p = l + t;

            rOut.X = rIn.X + sIn.X * p;
            rOut.Y = rIn.Y + sIn.Y * p;
            rOut.Z = rIn.Z + sIn.Z * p - d;

            return true;
        }


        public static VecD3 Refract(VecD3 r, VecD3 sIn,
            double nFront, double nBehind, double rho)
        {
            // auxiliary variables
            VecD3 n = new ()
            {
                X = -rho * r.X,
                Y = -rho * r.Y,
                Z = 1.0 - rho * r.Z
            };
            double cosI = VecD3.Dot(sIn, n);
            double rn = nFront / nBehind;
            double cosIP = Math.Sqrt(1.0 - Math.Pow(rn, 2.0) * (1.0 - cosI * cosI));
            double tau = nBehind * cosIP - nFront * cosI;
            double rtau = tau / nBehind;

            return new VecD3()
            {
                X = rn * sIn.X + rtau * n.X,
                Y = rn * sIn.Y + rtau * n.Y,
                Z = rn * sIn.Z + rtau * n.Z
            };
        }

        public static VecD3 ComputeSphericalSurfaceNormal(VecD3 r, double radius)
        {
            double rho = 1.0 / radius;
            return new VecD3()
            {
                X = -rho * r.X,
                Y = -rho * r.Y,
                Z = 1.0 - rho * r.Z
            };
        }

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


        public static VecD3 Reflect(VecD3 r, VecD3 sIn, double rho)
        {
            // auxiliary variables
            VecD3 n = new()
            {
                X = -rho * r.X,
                Y = -rho * r.Y,
                Z = 1.0 - rho * r.Z
            };
            double cosI = VecD3.Dot(sIn, n);
            return sIn - 2.0 * cosI * n;
        }

        #endregion
    }


    /// <summary>
    /// polarized ray class
    /// </summary>
    public class PolarizedRay : RayBase
    {
        #region properties

        /// <summary>
        /// complex field component Ex carried by the ray
        /// </summary>
        public Complex Ex { get; set; }

        /// <summary>
        /// complex field component Ey carried by the ray
        /// </summary>
        public Complex Ey { get; set; }

        //public Complex Hx { get; }
        //public Complex Hy { get; }
        //public Complex Ez { get; }
        //public Complex Hz { get; }

        #endregion

        public PolarizedRay(double wavelength, 
            Complex refractiveIndex) 
            : base(wavelength, refractiveIndex) { }

        public PolarizedRay(double wavelength, 
            Complex refractiveIndex,
            MathCore.VecD3 position, 
            MathCore.VecD3 direction)
            : base(wavelength, refractiveIndex, position, direction) { }

        public PolarizedRay(double wavelength, 
            Complex refractiveIndex,
            MathCore.VecD3 position, 
            MathCore.VecD3 direction, 
            Complex ex, Complex ey) 
            : base(wavelength, refractiveIndex, position, direction)
        {
            Ex = ex;
            Ey = ey;
        }

    }

    /// <summary>
    /// power ray class
    /// </summary>
    public class PowerRay : RayBase
    {
        /// <summary>
        /// power carried by the ray
        /// </summary>
        public double Power { get; set; }

        
        /// <summary>
        /// constructs a 
        /// </summary>
        /// <param name="wavelength"></param>
        /// <param name="refractiveIndex"></param>
        public PowerRay(double wavelength, Complex refractiveIndex)
            : base(wavelength, refractiveIndex) { }
    }

    /// <summary>
    /// the ray class 
    /// (no polarization considered)
    /// </summary>
    public class Ray : RayBase
    {
        /// <summary>
        /// power carried by the ray
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// constructs a 
        /// </summary>
        /// <param name="wavelength"></param>
        /// <param name="refractiveIndex"></param>
        public Ray(double wavelength, Complex refractiveIndex)
            : base(wavelength, refractiveIndex) { }


        public void Test()
        {
            
        }
    }


}
