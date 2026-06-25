using System;
using System.Collections.Generic;
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
    public class RKBPM2D
    {
        #region properties

        /// <summary>
        /// z span of the propagation
        /// </summary>
        public VectorD ZSpan { get; set; }

        /// <summary>
        /// medium function for z as input and Layer2DMedium as output
        /// </summary>
        public Func<double, Layer2DMedium> Medium { get; set; }

        /// <summary>
        ///  gridinfo in k-domain
        /// </summary>
        public GridInfo2D GridK { get; set; }

        /// <summary>
        /// gridinfo in x-domain
        /// </summary>
        public GridInfo2D GridX { get; set; }

        /// <summary>
        /// wavelength of the incident emfield
        /// </summary>
        public double Wavelength { get; set; }

        /// <summary>
        /// filter width in kx direction to get rid of evanescent wave
        /// </summary>
        public double FilterKx { get; set; }

        /// <summary>
        /// filter width in ky direction to get rid of evanescent wave
        /// </summary>
        public double FilterKy { get; set; }

        /// <summary>
        /// whether to save the Ex along the propagation
        /// </summary>
        public bool SaveExInside { get; set; }

        /// <summary>
        /// whether to save the Ey along the propagation
        /// </summary>
        public bool SaveEyInside { get; set; }

        /// <summary>
        /// whether to show the progress of RK solver
        /// </summary>
        public bool ShowProgress { get; set; }

        /// <summary>
        /// Ex inside the propagation
        /// </summary>
        public List<MatrixZ>? ExInside { get; set; }

        /// <summary>
        /// Ey inside the propagation
        /// </summary>
        public List<MatrixZ>? EyInside { get; set; }

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
        /// 
        /// </summary>
        /// <param name="zSpan">z span of the propagation</param>
        /// <param name="grinMedium">medium function</param>
        /// <param name="gridk">sample gridinfo in k-domain</param>
        /// <param name="wavelength">wavelength of the incident emfield</param>
        /// <param name="filterKx">half width of the filter in kx direction</param>
        /// <param name="filterKy">half width of the filter in ky direction</param>
        /// <param name="showProgress">whether to show the progress of RK solver</param>
        /// <param name="saveExInside">whether to save the Ex along the propagation</param>
        /// <param name="saveEyInside">whether to save the Ey along the propagation</param>
        public RKBPM2D(VectorD zSpan,
            Func<double, Layer2DMedium> grinMedium,
            GridInfo2D gridk,
            double wavelength,
            double filterKx,
            double filterKy = 0,
            bool showProgress = false,
            bool saveExInside = false,
            bool saveEyInside = false)
        {
            ZSpan = zSpan;
            Medium = grinMedium;
            Wavelength = wavelength;
            SaveExInside = saveExInside;
            SaveEyInside = saveEyInside;
            ShowProgress = showProgress;
            GridInfo2D gridx = new GridInfo2D(gridk);
            double a = 0.0;// 0 linear phase
            gridx.GetConjugated(FTOption.Backward, ref a, ref a);
            GridK = gridk;
            GridX = gridx;
            CalculatingTime = new TimeSpan();
            ConvertingTime = new TimeSpan();
            ConvolutionTime = new TimeSpan();
            MultiplicationTime = new TimeSpan();
            if (filterKy == 0) 
            { filterKy = filterKx; }
            FilterKx = filterKx;
            FilterKy = filterKy;
            if (saveExInside) { ExInside = new List<MatrixZ>(); }
            if (saveEyInside) { EyInside = new List<MatrixZ>(); }
        }
        #endregion

        #region propagate methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        internal MatrixZ Maxwell(double z, MatrixZ input)
        {
            var cal = CalculatingTime;
            var convo = ConvolutionTime;
            var multi = MultiplicationTime;
            var convert = ConvertingTime;
            var medium = Medium(z);
            var maxwell = new MaxwellODE.MaxwellODE2D(medium, GridK, GridX, Wavelength, FilterKx, FilterKy);
            var output = maxwell.dFz(input, ref cal, ref convert, ref convo, ref multi);
            CalculatingTime = cal;
            ConvolutionTime = convo;
            MultiplicationTime = multi;
            ConvertingTime  = convert;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ex">Ex field</param>
        /// <param name="Ey">Ey field</param>
        /// <param name="Hx">Hx field</param>
        /// <param name="Hy">Hy field</param>
        /// <exception cref="ArgumentException"></exception>
        public void Propagate(ref MatrixZ Ex, ref MatrixZ Ey, ref MatrixZ Hx, ref MatrixZ Hy)
        {
            // make sure the input fields have the same length
            if (Ex.Cols != Hx.Cols || Ex.Cols != Hy.Cols || Ex.Cols != Ey.Cols || Ex.Rows !=Hx.Rows || Ex.Rows != Hy.Rows || Ex.Rows != Ey.Rows)
            {
                throw new ArgumentException();
            }
            long n = Ex.Rows;

            // initialize the input matrix with E and H
            MatrixZ input = new MatrixZ(4 * n, Ex.Cols);
            input[new LongRange(0, n), new LongRange(0, input.Cols)] = Ex;
            input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)] = Ey;
            input[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)] = Hx;
            input[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)] = Hy;


            // solve the Maxwell equation with explicit Runge-Kutta method
            if (SaveExInside || SaveEyInside)
            {
                // loop over all variables in ZSpan
                for (long i = 0; i < (ZSpan.Count - 1); i++) 
                {
                    // current span of z with 2 points
                    var zspan = new VectorD(ZSpan[new LongRange(i, i + 2)]);
                    // take only 1 step of the RK method
                    RungeKutta.Explicit.Solve(Maxwell, ref input, zspan, showProgress: false);
                    // save the fields inside the propagation
                    if (SaveExInside) { ExInside.Add(new MatrixZ(input[new LongRange(0, n), new LongRange(0, input.Cols)])); }  
                    if (SaveEyInside) { EyInside.Add(new MatrixZ(input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)])); }  
                    // show the progress
                    if (ShowProgress)
                    {
                        double ii = i + 1;
                        double p = ii / ZSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                }
                // get the final E and H
                Ex = input[new LongRange(0, n), new LongRange(0, input.Cols)];
                Ey = input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)];
                Hx = input[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)];
                Hy = input[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)];
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
                // get the final E and H
                Ex = input[new LongRange(0, n), new LongRange(0, input.Cols)];
                Ey = input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)];
                Hx = input[new LongRange(2 * n, 3 * n), new LongRange(0, input.Cols)];
                Hy = input[new LongRange(3 * n, 4 * n), new LongRange(0, input.Cols)];
            }
        }

        /// <summary>
        /// propagate the fields along z direction co-work with IBVM
        /// </summary>
        /// <param name="input"> the vector contain Ex, Ey in front and Hx, Hy behind </param>
        /// <returns></returns>
        public MatrixZ PropagateIBVM(MatrixZ input)
        {
            if (input.Rows % 4 != 0) { throw new ArgumentException(); }
            long n = input.Rows / 4;

            // solve the Maxwell equation with explicit Runge-Kutta method
            if (SaveExInside || SaveEyInside)
            {
                // loop over all variables in ZSpan
                for (long i = 0; i < (ZSpan.Count - 1); i++)
                {
                    // current span of z with 2 points
                    var zspan = new VectorD(ZSpan[new LongRange(i, i + 2)]);
                    // take only 1 step of the RK method
                    RungeKutta.Explicit.Solve(Maxwell, ref input, zspan, showProgress: false);
                    // save the fields inside the propagation
                    if (SaveExInside) { ExInside.Add(new MatrixZ(input[new LongRange(0, n), new LongRange(0, input.Cols)])); }
                    if (SaveEyInside) { EyInside.Add(new MatrixZ(input[new LongRange(n, 2 * n), new LongRange(0, input.Cols)])); }
                    // show the progress
                    if (ShowProgress)
                    {
                        double ii = i + 1;
                        double p = ii / ZSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                    // get the final E and H
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
                // get the final E and H
                return input;
            }
        }
        #endregion
    }
}
