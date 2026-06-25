using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.RayTrace
{
    public class DiffractionEfficiency
    {

        /// <summary>
        /// computes the default grating depth
        /// </summary>
        /// <param name="wavelength_Design"> design wavelength </param>
        /// <param name="nFront_Design"> refractive index in front @ design wavelength </param>
        /// <param name="nBehind_Design"> refractive index behind @ design wavelength </param>
        /// <param name="order"> specified diffraction order </param>
        /// <returns> default blaze depth </returns>
        private static double DefaultDepth(double wavelength_Design,
            double nFront_Design, double nBehind_Design, int order)
        {
            double d = 0.0;
            if (nFront_Design != nBehind_Design)
                d = order * wavelength_Design / (nFront_Design - nBehind_Design);
            return d;
        }

        /// <summary>
        /// computes the scalar diffraction efficiency
        /// for the continuous blazed case i.e. kinoform
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="nFront"> refractive index in front @ working wavelength </param>
        /// <param name="nBehind"> refractive index behind @ working wavelength </param>
        /// <param name="order"> specified diffraction order </param>
        /// <param name="depth"> grating depth </param>
        /// <param name="angleIn"> angle of incidence </param>
        /// <param name="angleOut"> angle of diffraction </param>
        /// <returns> scalar diffraction efficiency </returns>
        private static double Blazed(double wavelength,
            double nFront, double nBehind,
            int order, double depth,
            double angleIn, double angleOut)
        {
            // auxiliary variables
            double beta = depth / wavelength
                * (nFront * angleIn - nBehind * angleOut);
            double temp = Math.PI * (beta - order);

            // compute efficiency
            double eta = Math.Sin(temp) / temp;
            return eta * eta;
        }

        /// <summary>
        /// computes the scalar diffraction efficiency
        /// for the stepped blazed case i.e. kinoform
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="nFront"> refractive index in front @ working wavelength </param>
        /// <param name="nBehind"> refractive index behind @ working wavelength </param>
        /// <param name="order"> specified diffraction order </param>
        /// <param name="depth"> grating depth </param>
        /// <param name="levels"> discrete levels of the stepped grating </param>
        /// <param name="angleIn"> angle of incidence </param>
        /// <param name="angleOut"> angle of diffraction </param>
        /// <returns> scalar diffraction efficiency </returns>
        private static double StepBlazed(double wavelength,
            double nFront, double nBehind,
            int order, double depth, int levels, 
            double angleIn, double angleOut)
        {
            // additional factor in comparison to the continuous case
            double fac = Math.Sin(order * Math.PI / levels) / (order * Math.PI);
            fac *= fac;

            // compute efficiency
            double eta = Blazed(wavelength, nFront, nBehind, 
                order, depth, angleIn, angleOut);
            return (fac * eta);
        }

        /// <summary>
        /// computes the scalar diffraction efficiency
        /// for the continuous sinusoidal case
        /// </summary>
        /// <param name="wavelength"> working wavelength </param>
        /// <param name="nFront"> refractive index in front @ working wavelength </param>
        /// <param name="nBehind"> refractive index behind @ working wavelength </param>
        /// <param name="order"> specified diffraction order </param>
        /// <param name="depth"> grating depth </param>
        /// <param name="angleIn"> angle of incidence </param>
        /// <param name="angleOut"> angle of diffraction </param>
        /// <returns> scalar diffraction efficiency </returns>
        private double Sinusoid(double wavelength,
            double nFront, double nBehind,
            int order, double depth, 
            double angleIn, double angleOut)
        {
            // auxiliary variables
            double beta = depth * (nFront * angleIn - nBehind * angleOut) / wavelength;

            // compute efficiency
            double eta = MathCore.SpecialFunctions.BesselJ(order, Math.PI * beta);
            return eta * eta;
        }


    }

    /// <summary>
    /// types of models for scalar diffraction
    /// efficiency calculation
    /// </summary>
    public enum EffType
    {
        Ideal = 0,
        Blazed = 10,
        BlazedDefault = 11,
        StepBlazed = 20,
        StepBlazedDefault = 21,
        Sinusoid = 30,
        SinusoidDefault = 31
    }


    /// <summary>
    /// a local linear grating kernel class
    /// with its direction along x
    /// and its normal along z
    /// </summary>
    public class LLGKernel
    {
        #region property

        /// <summary>
        /// period of the linear grating
        /// </summary>
        public double Period { get; set; }

        /// <summary>
        /// wavelength under investigation
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// refractive index in front
        /// light comes from front
        /// reflects back to front medium
        /// </summary>
        public Complex NFront { get; set; }

        /// <summary>
        /// refractive index behind
        /// light transmits to medium behind
        /// </summary>
        public Complex NBehind { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public LLGKernel() { }

        #endregion
        #region methods

        /// <summary>
        /// computes output kx for a given order
        /// </summary>
        /// <param name="kxIn"> kx-component of the incident plane wave </param>
        /// <param name="order"> order index (not differentiating reflection/transmission) </param>
        /// <returns></returns>
        public double ComputeKxOut(double kxIn, int order)
            => kxIn + order * 2.0 * Math.PI / Period;

        /// <summary>
        /// checks whether a kx is propagating or not
        /// </summary>
        /// <param name="kx"> kx-component </param>
        /// <param name="isReflection"> true for reflection mode; otherwise transmission </param>
        /// <returns></returns>
        public bool IsKxPropagating(double kx, bool isReflection)
        {
            // define the limiting k
            double k;
            if (isReflection)
                k = 2.0 * Math.PI / Wavelength * NFront.Real;
            else
                k = 2.0 * Math.PI / Wavelength * NBehind.Real;

            // check value
            if (Math.Abs(kx) > k)
                return false;
            else
                return true;
        }

        #endregion
    }


}
