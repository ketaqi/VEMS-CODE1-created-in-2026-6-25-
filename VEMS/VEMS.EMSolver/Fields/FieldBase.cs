using Complex = System.Numerics.Complex;

namespace VEMS.EMSolver
{

    /// <summary>
    /// monochromatic single-mode
    /// electromagnetic field base object
    /// given in a homogenous isotropic medium
    /// </summary>
    public class FieldBase //: IField
    {

        #region properties

        /// <summary>
        /// internally stored wavelength in vacuum
        /// </summary>
        private double wavelength { get; set; }

        /// <summary>
        /// internally stored wavenumber
        /// </summary>
        private double k0 { get; set; }

        /// <summary>
        /// get / set the vacuum wavelength
        /// </summary>
        public double Wavelength 
        { 
            get =>  wavelength;
            set
            {
                if(wavelength != value)
                {
                    wavelength = value;
                    k0 = 2.0 * Math.PI / wavelength;
                }

            }
        }

        /// <summary>
        /// wavenumber in vacuum 
        /// K0 = 2.0 * PI / Wavelength
        /// </summary>
        public double K0
        {
            get => k0;
            set
            {
                if(k0 != value)
                {
                    k0 = value;
                    wavelength = 2.0 * Math.PI / k0;
                }
            }
        }

        /// <summary>
        /// get / set the complex permittivity 
        /// of the embedding medium
        /// </summary>
        public Complex Epsilon { get; set; }

        /// <summary>
        /// get / set the complex permeability
        /// of the embedding medium
        /// </summary>
        public Complex Mu { get; set; } = 1.0;

        /// <summary>
        /// get the complex refractive index
        /// </summary>
        public Complex RefractiveIndex => Complex.Sqrt(Epsilon * Mu);

        #endregion
        #region constructors 

        /// <summary>
        /// empty constructor
        /// </summary>
        public FieldBase() { }

        /// <summary>
        /// constructs an EM field
        /// with given wavelength
        /// in default medium vacuum
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        public FieldBase(double wavelength)
        {
            Wavelength = wavelength;
            Epsilon = Complex.One;
            Mu = Complex.One;
        }

        /// <summary>
        /// constructs an EM field
        /// with given wavelength, permittivity
        /// and permeability set to 1.0 by default
        /// </summary>
        /// <param name="wavelength"> wavelength in vacuum </param>
        /// <param name="epsilon"> permittivity </param>
        public FieldBase(double wavelength, Complex epsilon)
        {
            Wavelength = wavelength;
            Epsilon = epsilon;
            Mu = Complex.One;
        }

        /// <summary>
        /// constructs an EMField
        /// with given parameters
        /// </summary>
        /// <param name="wavelength"> vacuum wavelength </param>
        /// <param name="epsilon"> permittivity </param>
        /// <param name="mu"> permeability</param>
        public FieldBase(double wavelength,
            Complex epsilon, Complex mu)
        {
            Wavelength = wavelength;
            Epsilon = epsilon;
            Mu = mu;
        }

        #endregion
        #region methods
        // ...
        #endregion

    }

    /// <summary>
    /// field interface
    /// </summary>
    public interface IField
    {
        #region properties

        /// <summary>
        /// wavelength in vacuum
        /// </summary>
        double Wavelength { get; set; } 

        /// <summary>
        /// wavenumber in vacuum
        /// k0 = 2.0 * PI / wavelength
        /// </summary>
        double K0 { get => 2.0 * Math.PI / Wavelength; }

        /// <summary>
        /// embedding material
        /// </summary>
        Material Material { get; set; }

        /// <summary>
        /// modeling domain
        /// </summary>
        ModelingDomain Domain { get; set; }

        /// <summary>
        /// overall scaling factor
        /// </summary>
        Complex Scaling { get; set; }

        #endregion
        #region constructors

        // ...

        #endregion
        #region methods

        // ...

        #endregion
    }


    /// <summary>
    /// sign factor according to propagation direction
    /// +1 for positive-direction propagation
    /// -1 for negative-direction propagation
    /// </summary>
    public enum SignFactor : int
    {
        /// <summary>
        /// positive-direction propagation
        /// </summary>
        Positive = 1,

        /// <summary>
        /// negative-direction propagation
        /// </summary>
        Negative = -1

        //Test = 101
    }

    /// <summary>
    /// polarization mode option
    /// TE or TM mode
    /// </summary>
    public enum PolarizationMode
    {
        /// <summary>
        /// TE - transverse electric mode
        /// </summary>
        TE = 0,

        /// <summary>
        /// TM - transverse magnetic mode
        /// </summary>
        TM = 1
    }

    /// <summary>
    /// modeling domain of the electromagnetic fields
    /// </summary>
    public enum ModelingDomain
    {
        /// <summary>
        /// spatial domain
        /// </summary>
        Spatial = 0,

        /// <summary>
        /// spatial frequency domain (k domain)
        /// </summary>
        SpatialFrequency = 1
    }

    /// <summary>
    /// Jones vector (complex-valued)
    /// </summary>
    public struct JonesVector
    {
        #region properties

        /// <summary>
        /// x-component of the Jones vector
        /// </summary>
        public Complex Jx { get; set; }

        /// <summary>
        /// y-component of the Jones vector
        /// </summary>
        public Complex Jy { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// constructs a Jones vector
        /// with given x and y components
        /// </summary>
        /// <param name="jx"> x component of the vector </param>
        /// <param name="jy"> y component of the vector </param>
        public JonesVector(Complex jx, Complex jy)
        {
            Jx = jx;
            Jy = jy;
        }

        /// <summary>
        /// constructs a Jones vector
        /// with given x and y components
        /// </summary>
        /// <param name="jx"> x component of the vector </param>
        /// <param name="jy"> y component of the vector </param>
        public JonesVector(double jx, double jy)
        {
            Jx = new Complex(jx, 0.0);
            Jy = new Complex(jy, 0.0);
        }

        #endregion
        #region methods

        /// <summary>
        /// normalizes the Jones vector
        /// </summary>
        public void Normalize()
        {
            double jxAbs = Jx.Magnitude;
            double jyAbs = Jy.Magnitude;
            double jAbs = Math.Sqrt(jxAbs * jxAbs + jyAbs * jyAbs);
            Jx /= jAbs;
            Jy /= jAbs;
        }

        #endregion
        #region static methods

        /// <summary>
        /// linear Ex polarization vector
        /// </summary>
        public static JonesVector LinearEx
            => new JonesVector(1.0, 0.0);

        /// <summary>
        /// linear Ey polarization vector
        /// </summary>
        public static JonesVector LinearEy
            => new JonesVector(0.0, 1.0);

        /// <summary>
        /// linear 45 degree polarization vector
        /// </summary>
        public static JonesVector Linear45
            => new JonesVector(0.5 * Math.Sqrt(2.0), 0.5 * Math.Sqrt(2.0));

        /// <summary>
        /// left circular polarization vector
        /// </summary>
        public static JonesVector CircularLeft
            => new JonesVector();

        /// <summary>
        /// right circular polarization vector
        /// </summary>
        public static JonesVector CircularRight
            => new JonesVector();

        #endregion

    }

}
