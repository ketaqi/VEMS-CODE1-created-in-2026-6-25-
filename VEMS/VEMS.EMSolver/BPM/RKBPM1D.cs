using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.EMSolver
{

    /// <summary>
    /// Runge-Kutta-based BPM
    /// </summary>
        public class RKBPM1D
    {
        #region properties

        /// <summary>
        /// medium of the propagation
        /// </summary>
        public Func<double, Layer1DMedium> Medium { get; set; }

        /// <summary>
        /// z span of the propagation
        /// </summary>
        public VectorD ZSpan { get; set; }

        /// <summary>
        /// gridinfo in k-domain
        /// </summary>
        public GridInfo1D GridK { get; set; }

        /// <summary>
        /// half width of the k-domain filter to get rid of evenescent wave
        /// </summary>
        public double FilterWidth { get; set; }

        /// <summary>
        /// gridinfo in x-domin
        /// </summary>
        public GridInfo1D GridX { get; set; }

        /// <summary>
        /// wavelength of the incident emfield
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// polarization of the incident emfield
        /// </summary>
        InPlanePolMode Polarization { get; set; }

        /// <summary>
        /// whether to save the fields along the propagation
        /// </summary>
        public bool SaveFieldsInside { get; set; }

        /// <summary>
        /// whether to show the progress of RK solver
        /// </summary>
        public bool ShowProgress { get; set; }

        /// <summary>
        /// fields inside the propagation
        /// </summary>
        public List<VectorZ>? FieldsInside { get; set; }

        /// <summary>
        /// time spent on RK-BPM calculating
        /// </summary>
        public TimeSpan CalculatingTime { get; set; }

        /// <summary>
        /// time spent on convolution during RK-BPM
        /// </summary>
        public TimeSpan ConvertingTime { get; set; }

        /// <summary>
        /// time spent on convolution during RK-BPM
        /// </summary>
        public TimeSpan ConvolutionTime { get; set; }

        /// <summary>
        /// time spent on multiplication during RK-BPM
        /// </summary>
        public TimeSpan MultiplicationTime { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="zSpan"> z span of the propagation </param>
        /// <param name="grinMedium"> GRIN medium  </param>
        /// <param name="gridk"> gridinfo in k-domain </param>
        /// <param name="wavelength"> wavelength of the incident emfield </param>
        /// <param name="filterK"> half width of the k-domain filter to get rid of evenescent wave </param>
        /// <param name="polarization"> polarization of the incident emfield </param>
        /// <param name="showProgress"> whether to show the progress of RK solver </param>
        /// <param name="saveFieldsInside"> whether to save the fields along the propagation </param>
        public RKBPM1D(VectorD zSpan,
            Func<double, Layer1DMedium> grinMedium,            
            GridInfo1D gridk,
            double wavelength,
            double filterK,
            InPlanePolMode polarization = InPlanePolMode.TE,
            bool showProgress = false,
            bool saveFieldsInside = false)
        {
            Medium = grinMedium;
            ZSpan = zSpan;
            SaveFieldsInside = saveFieldsInside;
            ShowProgress = showProgress;
            GridInfo1D gridx = new GridInfo1D(gridk);
            double a = 0.0;// 0 linear phase
            gridx.GetConjugated(FTOption.Backward, ref a);
            GridK = gridk;
            GridX = gridx;
            FilterWidth = filterK;
            Wavelength = wavelength;
            Polarization = polarization;
            if (saveFieldsInside) { FieldsInside = new List<VectorZ>(); }
            CalculatingTime = new TimeSpan();
            ConvertingTime = new TimeSpan();
            ConvolutionTime = new TimeSpan();
            MultiplicationTime = new TimeSpan();
        }


        #endregion

        #region propagate methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        internal VectorZ Maxwell(double z, VectorZ input)
        {
            var cal = CalculatingTime;
            var convo = ConvolutionTime;
            var multi = MultiplicationTime;
            var convert = ConvertingTime;
            var medium = Medium(z);
            var maxwell = new MaxwellODE.MaxwellODE1D(medium, GridX, GridK, Wavelength, FilterWidth, Polarization);
            var output = maxwell.dFz(input, ref cal, ref convert, ref convo, ref multi);
            CalculatingTime = cal;
            ConvolutionTime = convo;
            MultiplicationTime = multi;
            ConvertingTime = convert;
            return output;
        }

        /// <summary>
        /// propagate the fields along z direction
        /// </summary>
        /// <param name="E"> electrofield input in k-domain </param>
        /// <param name="H"> magneticfield input in k-domain </param>
        public void Propagate(ref VectorZ E, ref VectorZ H)
        {
            // make sure the input fields have the same length
            if (E.Count != H.Count)
            {
                throw new ArgumentException();
            }
            long n = E.Count;

            // initialize the input vector with E and H
            VectorZ input = new VectorZ(2 * n);
            input[new LongRange(0, n)] = E;
            input[new LongRange(n, 2 * n)] = H;

            // solve the Maxwell equation with explicit Runge-Kutta method
            if (SaveFieldsInside)
            {
                // loop over all variables in ZSpan
                for (long i = 0; i < (ZSpan.Count - 1); i++)
                {
                    // current span of z with 2 points
                    var zspan = new VectorD(ZSpan[new LongRange(i, i + 2)]);
                    // take only 1 step of the RK method
                    RungeKutta.Explicit.Solve(Maxwell, ref input, zspan, showProgress: false);
                    // save the fields inside the propagation
                    FieldsInside.Add(new VectorZ(input[new LongRange(0, n)]));
                    // show the progress
                    if (ShowProgress)
                    {
                        double ii = i + 1;
                        double p = ii / ZSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                }
                // get the final E and H
                E = input[new LongRange(0, n)];
                H = input[new LongRange(n, 2 * n)];
            }
            else
            {
                // solve the Maxwell equation with explicit Runge-Kutta method
                if(ShowProgress)
                {
                    RungeKutta.Explicit.Solve(Maxwell, ref input, ZSpan, showProgress: true);
                }
                else
                {
                    RungeKutta.Explicit.Solve(Maxwell, ref input, ZSpan, showProgress: false);
                }
                // get the final E and H
                E = input[new LongRange(0, n)];
                H = input[new LongRange(n, 2 * n)];
            }
        }

        /// <summary>
        /// propagate the fields along z direction co-work with IBVM
        /// </summary>
        /// <param name="input"> the vector contain E in front and H behind </param>
        /// <returns></returns>
        public VectorZ PropagateIBVM (VectorZ input)
        {
            if (input.Count % 2 != 0) { throw new ArgumentException(); }
            long n = input.Count / 2;

            // solve the Maxwell equation with explicit Runge-Kutta method
            if (SaveFieldsInside)
            {
                // loop over all variables in ZSpan
                for (long i = 0; i < (ZSpan.Count - 1); i++)
                {
                    // current span of z with 2 points
                    var zspan = new VectorD(ZSpan[new LongRange(i, i + 2)]);
                    // take only 1 step of the RK method
                    RungeKutta.Explicit.Solve(Maxwell, ref input, zspan, showProgress: false);
                    // save the fields inside the propagation
                    FieldsInside.Add(new VectorZ(input[new LongRange(0, n)]));
                    // show the progress
                    if (ShowProgress)
                    {
                        double ii = i + 1;
                        double p = ii / ZSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                }
                return input;
            }
            else
            {
                // solve the Maxwell equation with explicit Runge-Kutta method
                if (ShowProgress)
                {
                    RungeKutta.Explicit.Solve(Maxwell, ref input, ZSpan, showProgress: true);
                }
                else
                {
                    RungeKutta.Explicit.Solve(Maxwell, ref input, ZSpan, showProgress: false);
                }
                return input;
            }
        }
        #endregion
    }
}



