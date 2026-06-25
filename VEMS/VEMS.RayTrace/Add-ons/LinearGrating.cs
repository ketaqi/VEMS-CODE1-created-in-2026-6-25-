using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.RayTrace
{
    /// <summary>
    /// linear grating model
    /// according to CodeV
    /// </summary>
    public class LinearGrating
    {
        #region properties

        /// <summary>
        /// selected diffraction order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// the spacing between the generating planes 
        /// </summary>
        public double GratingSpacing { get; set; }

        /// <summary>
        /// unit vector direction of the normal 
        /// to the generating planes to establish the rulings
        /// </summary>
        public VecD3 GratingDirection { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default linear grating constructor
        /// </summary>
        public LinearGrating()
        {
            Order = 0;
            GratingSpacing = 1.0E-3; // 1.0 mm 
            GratingDirection = VecD3.UnitY;
        }

        /// <summary>
        /// constructs a linear grating class
        /// with given order, spacing and direction
        /// </summary>
        /// <param name="order"> selected diffraction order </param>
        /// <param name="gratingSpacing"> grating spacing </param>
        /// <param name="gratingDirection"> grating direction [unit vector] </param>
        public LinearGrating(int order,
            double gratingSpacing,
            VecD3 gratingDirection)
        {
            Order = order;
            GratingDirection = gratingDirection;
            GratingSpacing = gratingSpacing;
        }

        #endregion
        #region methods

        /// <summary>
        /// computes the local grating information
        /// </summary>
        /// <param name="n"> local surface normal vector [unit vector] </param>
        /// <returns> (local direction, local spacing) </returns>
        private (VecD3, double) ComputeLocalGratingInfo(VecD3 n)
        {
            VecD3 localDirection = GratingDirection - VecD3.Dot(GratingDirection, n) * n;
            localDirection.Normalize();
            double localSpacing = GratingSpacing / VecD3.Dot(GratingDirection, localDirection);
            return (localDirection, localSpacing);
        }

        /// <summary>
        /// computes diffracted ray direction
        /// after intersection is found and
        /// surface normal is computed
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="sIn"> incoming ray direction [unit vector] </param>
        /// <param name="nFront"> refractive index in front </param>
        /// <param name="nBehind"> refractive index behind </param>
        /// <param name="n"> surface normal at the intersection [unit vector] </param>
        /// <returns> diffracted ray direction [unit vector] </returns>
        public VecD3? Diffract(double wavelength, VecD3 sIn,
            double nFront, double nBehind, VecD3 n)
        {
            // compute local grating info
            (VecD3 localDirection, double localSpacing) = ComputeLocalGratingInfo(n);
            
            // aux variable u: (formula below)
            VecD3 u = 1.0 / nBehind * (nFront * sIn + Order * wavelength / localSpacing * localDirection);
            // aux variable nu: n dot u
            double nu = VecD3.Dot(n, u);
            // aux variable delta: the part in the sqrt operator ...
            double delta = 1.0 - VecD3.NormSquare(u) + nu * nu;

            // check delta
            if (delta < 0.0) { return null; }

            // when delta >= 0.0
            // diffraction direction
            return u + (Math.Sqrt(delta) - nu) * n;
        }

        /// <summary>
        /// computes diffracted ray direction
        /// after intersection is found and
        /// surface normal is computed
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="sIn"> incoming ray direction [unit vector] </param>
        /// <param name="nFront"> refractive index in front </param>
        /// <param name="nBehind"> refractive index behind </param>
        /// <param name="x"> x-component of the intersection on the surface </param>
        /// <param name="y"> y-component of the intersection on the surface </param>
        /// <param name="n"> surface normal at the intersection [unit vector] </param>
        /// <param name="sOut"> diffracted ray direction [unit vector] </param>
        /// <param name="dPsi"> change of phase </param>
        /// <returns> if success, return true; else, false </returns>
        public bool Diffract(double wavelength, VecD3 sIn,
            double nFront, double nBehind, double x, double y, VecD3 n,
            out VecD3 sOut, out double dPsi)
        {
            // initialize output
            sOut = new VecD3();
            dPsi = 0.0;
            // compute local grating info
            (VecD3 localDirection, double localSpacing) = ComputeLocalGratingInfo(n);
            // aux variable u: (below)
            VecD3 u = 1.0 / nBehind * (nFront * sIn + Order * wavelength / localSpacing * localDirection);
            // aux variable nu: n dot u
            double nu = VecD3.Dot(n, u);
            // aux variable delta: the part in the sqrt operator ...
            double delta = 1.0 - VecD3.NormSquare(u) + nu * nu;

            // check delta
            if (delta < 0.0) { return false; }

            // when delta >= 0.0
            // diffraction direction
            sOut = u + (Math.Sqrt(delta) - nu) * n;

            //compute local period on the reference x - y plane
            double fxy = GratingDirection.X * GratingDirection.X
                + GratingDirection.Y * GratingDirection.Y;
            fxy = Math.Sqrt(fxy);
            double dx = GratingDirection.X / fxy * GratingSpacing;
            double dy = GratingDirection.Y / fxy * GratingSpacing;
            dPsi = 2.0 * Math.PI * Order * (x / dx + y / dy);

            return true;
        }

        #endregion

        #region obsolete ...

        /// <summary>
        /// computes the direction of a selected diffraction order
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="sIn"> input ray direction </param>
        /// <param name="order"> selected diffraction order </param>
        /// <param name="x"> location x </param>
        /// <param name="y"> location y </param>
        /// <param name="n"> local surface normal vector at (x, y, h) </param>
        /// <returns> diffracted ray direction and phase change </returns>
        [Obsolete]
        public (VecD3, double) ComputeDiffractionInfo(double wavelength,
            double nFront,
            double nBehind,
            VecD3 sIn,
            int order,
            double x,
            double y,
            VecD3 n)
        {
            // compute local grating info
            (VecD3 localDirection, double localSpacing) = ComputeLocalGratingInfo(n);
            // aux variable u
            VecD3 u = 1.0 / nBehind * (nFront * sIn
                + order * wavelength / localSpacing * localDirection);

            // aux variable n.u
            double nu = VecD3.Dot(n, u);

            // diffraction direction
            VecD3 sOut = u + (Math.Sqrt(1.0 - VecD3.NormSquare(u) + nu * nu) - nu) * n;

            // phase change
            double dPsi = 0.0; // (x / Dx + y / Dy) * 2.0 *Math.PI * order;

            return (sOut, dPsi);
        }

        /// <summary>
        /// example on how to use the scalar diffraction 
        /// efficiency calculators for different cases
        /// </summary>
        /// <param name="type"> model type option </param>
        /// <param name="wavelength"> wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="depth"> blaze depth </param>
        /// <param name="n"> local surface normal vector </param>
        /// <param name="sIn"> input ray direction </param>
        /// <param name="sOut"> output ray direction </param>
        /// <param name="order"> diffraction order; default is 1 </param>
        /// <param name="levels"> number of discrete levels </param>
        /// <returns> scalar diffraction efficiency </returns>
        [Obsolete]
        public double ComputeScalarEff(EffType type,
            double wavelength, double nFront, double nBehind,
            double wavelengthDesign, double nFrontDesign, double nBehindDesign,
            double depth, VecD3 n, VecD3 sIn, VecD3 sOut,
            int order = 1, int levels = int.MaxValue)
        {
            if (type == EffType.Ideal)
                return 1.0;

            double thetaIn = VecD3.Dot(n, sIn);
            double thetaOut = VecD3.Dot(n, sOut);

            switch (type)
            {
                case EffType.Blazed:
                    return ComputeBlazedEfficiency(wavelength, nFront, nBehind,
                        depth, thetaIn, thetaOut, order);
                case EffType.BlazedDefault:
                    return ComputeBlazedEfficiency_DefaultDepth(wavelength, nFront, nBehind,
                        wavelengthDesign, nFrontDesign, nBehindDesign,
                        thetaIn, thetaOut, order);
                case EffType.StepBlazed:
                    return ComputeStepBlazedEff(wavelength, nFront, nBehind,
                        depth, thetaIn, thetaOut, order, levels);
                case EffType.StepBlazedDefault:
                    return ComputeStepBlazedEff(wavelength, nFront, nBehind,
                        thetaIn, thetaOut, order, levels);
                case EffType.Sinusoid:
                    return ComputeSinusoidEff(wavelength, nFront, nBehind,
                        depth, thetaIn, thetaOut, order);
                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// computes the scalar diffraction efficiency
        /// </summary>
        /// <param name="type"> model type option </param>
        /// <param name="wavelength"> (working) wavelength </param>
        /// <param name="nFront"> refractive index in front @(working) wavelength </param>
        /// <param name="nBehind"> refractive index behind @(working) wavelength </param>
        /// <param name="wavelengthDesign"> design wavelength </param>
        /// <param name="nFrontDesign"> refractive index in front @design wavelength </param>
        /// <param name="nBehindDesign"> refractive index behind @design wavelength </param>
        /// <param name="depth"> grating depth (invalid when set to models with default depth) </param>
        /// <param name="n"> local surface normal vector </param>
        /// <param name="sIn"> input ray direction </param>
        /// <param name="sOut"> output ray direction </param>
        /// <param name="order"> diffraction order; default is 1 </param>
        /// <param name="levels"> number of discrete depth levels, valid only for stepped blazed grating </param>
        /// <returns> scalar diffraction efficiency </returns>
        public double ComputeScalarEfficiency(EffType type,
            double wavelength, double nFront, double nBehind,
            double wavelengthDesign, double nFrontDesign, double nBehindDesign,
            double depth, VecD3 n, VecD3 sIn, VecD3 sOut,
            int order = 1, int levels = int.MaxValue)
        {
            if (type == EffType.Ideal)
                return 1.0;

            double thetaIn = VecD3.Dot(n, sIn);
            double thetaOut = VecD3.Dot(n, sOut);

            switch (type)
            {
                case EffType.Blazed:
                    return ComputeBlazedEfficiency(wavelength, nFront, nBehind,
                        depth, thetaIn, thetaOut, order);
                case EffType.BlazedDefault:
                    {
                        double d = 0.0;
                        if (nFrontDesign != nBehindDesign)
                            d = order * wavelengthDesign / (nFrontDesign - nBehindDesign);
                        return ComputeBlazedEfficiency(wavelength, nFront, nBehind,
                            d, thetaIn, thetaOut, order);
                    }
                case EffType.StepBlazed:
                    return ComputeStepBlazedEfficiency(wavelength, nFront, nBehind,
                        depth, thetaIn, thetaOut, order, levels);
                case EffType.StepBlazedDefault:
                    {
                        double d = 0.0;
                        if (nFrontDesign != nBehindDesign)
                            d = order * wavelengthDesign / (nFrontDesign - nBehindDesign);
                        return ComputeStepBlazedEfficiency(wavelength, nFront, nBehind,
                            d, thetaIn, thetaOut, order, levels);
                    }
                case EffType.Sinusoid:
                    return ComputeSinusoidEfficiency(wavelength, nFront, nBehind,
                        depth, thetaIn, thetaOut, order);
                case EffType.SinusoidDefault:
                    {
                        double d = 0.0;
                        if (nFrontDesign != nBehindDesign)
                            d = order * wavelengthDesign / (nFrontDesign - nBehindDesign);
                        return ComputeSinusoidEfficiency(wavelength, nFront, nBehind,
                            depth, thetaIn, thetaOut, order);
                    }
                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// computes scalar diffraction efficiency 
        /// for the ideal case
        /// </summary>
        /// <returns> return 100% </returns>
        private double ComputeIdealEff()
            => 1.0;

        /// <summary>
        /// computes scalar diffraction efficiency 
        /// for the continuous blazed case 
        /// </summary>
        /// <param name="wavelength"> wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="depth"> blaze depth </param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction </param>
        /// <param name="order"> diffraction order </param>
        /// <returns> scalar diffraction efficiency </returns>
        [Obsolete]
        private double ComputeBlazedEff(double wavelength, double nFront, double nBehind,
            double depth, double thetaIn, double thetaOut, int order)
        {
            double beta = depth / wavelength * (nFront * thetaIn - nBehind * thetaOut);
            double temp = Math.PI * (beta - order);
            double eta = Math.Sin(temp) / temp;
            return eta * eta;
        }
        /// <summary>
        /// computes the scalar diffraction efficiency
        /// for the continuous blazed case
        /// </summary>
        /// <param name="wavelengthWork"> working wavelength </param>
        /// <param name="nFrontWork"> refractive index in front @working wavelength </param>
        /// <param name="nBehindWork"> refractive index behind @working wavelength </param>
        /// <param name="depth"> manually defined grating depth </param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction @specified order </param>
        /// <param name="order"> specified diffraction order </param>
        /// <returns> scalar diffraction efficiency </returns>
        private double ComputeBlazedEfficiency(double wavelengthWork,
            double nFrontWork, double nBehindWork,
            double depth, double thetaIn, double thetaOut, int order)
        {
            double beta = depth / wavelengthWork
                * (nFrontWork * thetaIn - nBehindWork * thetaOut);
            double temp = Math.PI * (beta - order);
            double eta = Math.Sin(temp) / temp;
            return eta * eta;
        }

        /// <summary>
        /// computes scalar diffraction efficiency 
        /// for the continuous blazed case with
        /// default blaze depth
        /// </summary>
        /// <param name="wavelength"> wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction </param>
        /// <param name="order"> diffraction order </param>
        /// <returns> scalar diffraction efficiency </returns>
        [Obsolete]
        private double ComputeBlazedEff(double wavelength, double nFront, double nBehind,
            double thetaIn, double thetaOut, int order)
        {
            double d = 0.0;
            if (nFront != nBehind)
                d = order * wavelength / (nFront - nBehind);
            return ComputeBlazedEff(wavelength, nFront, nBehind,
                d, thetaIn, thetaOut, order);
        }
        /// <summary>
        /// computes the scalar diffraction efficiency
        /// for the continuous blazed case
        /// with default blazed depth at the design wavelength
        /// </summary>
        /// <param name="wavelengthWork"> working wavelength </param>
        /// <param name="nFrontWork"> refractive index in front @working wavelength </param>
        /// <param name="nBehindWork"> refractive index behind @working wavelength </param>
        /// <param name="wavelengthDesign"> design wavelength </param>
        /// <param name="nFrontDesign"> refractive index in front @design wavelength</param>
        /// <param name="nBehindDesign"> refractive index behind @design wavelength</param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction @specified order </param>
        /// <param name="order"> specified diffraction order </param>
        /// <returns> scalar diffraction efficiency </returns>
        private double ComputeBlazedEfficiency_DefaultDepth(double wavelengthWork,
            double nFrontWork, double nBehindWork,
            double wavelengthDesign,
            double nFrontDesign, double nBehindDesign,
            double thetaIn, double thetaOut, int order)
        {
            double d = 0.0;
            if (nFrontDesign != nBehindDesign)
                d = order * wavelengthDesign / (nFrontDesign - nBehindDesign);
            return ComputeBlazedEfficiency(wavelengthWork,
                nFrontWork, nBehindWork,
                d, thetaIn, thetaOut, order);
        }


        /// <summary>
        /// computes scalar diffraction efficiency
        /// for the step blazed case
        /// </summary>
        /// <param name="wavelength"> wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="depth"> blaze depth </param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction </param>
        /// <param name="order"> diffraction order </param>
        /// <param name="levels"> number of discrete levels </param>
        /// <returns> scalar diffraction efficiency </returns>
        [Obsolete]
        private double ComputeStepBlazedEff(double wavelength, double nFront, double nBehind,
            double depth, double thetaIn, double thetaOut, int order, int levels)
        {
            double beta = depth * (nFront * thetaIn - nBehind * thetaOut) / wavelength;
            double temp = Math.PI * (beta - order);
            double fac = Math.Sin(order * Math.PI / levels) / (order * Math.PI);
            fac *= fac;
            double eta = Math.Sin(temp) / Math.Sin(temp / levels);
            eta *= eta;
            return (fac * eta);
        }

        private double ComputeStepBlazedEfficiency(double wavelengthWork,
            double nFrontWork, double nBehindWork,
            double depth, double thetaIn, double thetaOut,
            int order, int levels)
        {
            double beta = depth * (nFrontWork * thetaIn - nBehindWork * thetaOut) / wavelengthWork;
            double temp = Math.PI * (beta - order);
            double fac = Math.Sin(order * Math.PI / levels) / (order * Math.PI);
            fac *= fac;
            double eta = Math.Sin(temp) / Math.Sin(temp / levels);
            eta *= eta;
            return (fac * eta);
        }


        /// <summary>
        /// computes scalar diffraction efficiency
        /// for the step blazed case with
        /// default blaze depth
        /// </summary>
        /// <param name="wavelength"> wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction </param>
        /// <param name="order"> diffraction order </param>
        /// <param name="levels"> number of discrete levels </param>
        /// <returns> scalar diffraction efficiency </returns>
        [Obsolete]
        private double ComputeStepBlazedEff(double wavelength, double nFront, double nBehind,
            double thetaIn, double thetaOut, int order, int levels)
        {
            double d = 0.0;
            if (nFront != nBehind)
                d = order * wavelength / (nFront - nBehind);
            return ComputeStepBlazedEff(wavelength, nFront, nBehind,
                d, thetaIn, thetaOut, order, levels);
        }

        private double ComputeStepBlazedEfficiency_DefaultDepth(double wavelengthWork,
            double nFrontWork, double nBehindWork,
            double wavelengthDesign,
            double nFrontDesign, double nBehindDesign,
            double thetaIn, double thetaOut, int order, int levels)
        {
            double d = 0.0;
            if (nFrontDesign != nBehindDesign)
                d = order * wavelengthDesign / (nFrontDesign - nBehindDesign);
            return ComputeStepBlazedEfficiency(wavelengthWork,
                nFrontWork, nBehindWork,
                d, thetaIn, thetaOut, order, levels);
        }

        /// <summary>
        /// !!! not finished
        /// computes scalar diffraction efficiency
        /// for the sinusoid case
        /// </summary>
        /// <param name="wavelength"> wavelength </param>
        /// <param name="nFront"> refractive index in front (real-part only) </param>
        /// <param name="nBehind"> refractive index behind (real-part only) </param>
        /// <param name="depth"> blaze depth </param>
        /// <param name="thetaIn"> angle of incidence </param>
        /// <param name="thetaOut"> angle of diffraction </param>
        /// <param name="order"> diffraction order </param>
        /// <returns> scalar diffraction efficiency </returns>
        [Obsolete]
        private double ComputeSinusoidEff(double wavelength, double nFront, double nBehind,
            double depth, double thetaIn, double thetaOut, int order)
        {
            double beta = depth * (nFront * thetaIn - nBehind * thetaOut) / wavelength;
            // need Bessel function of the first kind ...
            return double.NaN;
        }


        private double ComputeSinusoidEfficiency(double wavelengthWork,
            double nFrontWork, double nBehindWork,
            double depth, double thetaIn, double thetaOut, int order)
        {
            double beta = depth * (nFrontWork * thetaIn - nBehindWork * thetaOut) / wavelengthWork;
            double eta = MathCore.SpecialFunctions.BesselJ(order, Math.PI * beta);
            return eta * eta;
        }


        private double ComputeSinusoidEfficiency_DefaultDepth(double wavelengthWork,
            double nFrontWork, double nBehindWork,
            double wavelengthDesign,
            double nFrontDesign, double nBehindDesign,
            double thetaIn, double thetaOut, int order, int levels)
        {
            double d = 0.0;
            if (nFrontDesign != nBehindDesign)
                d = order * wavelengthDesign / (nFrontDesign - nBehindDesign);
            return ComputeSinusoidEfficiency(wavelengthWork, nFrontWork, nBehindWork,
                d, thetaIn, thetaOut, order);
        }

        #endregion
        #region static methods



        #endregion
    }

}
