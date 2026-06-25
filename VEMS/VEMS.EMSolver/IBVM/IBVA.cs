using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    /// <summary>
    /// Iterative Boundary-value Matching Algorithm
    /// [currently for single spatial frequency only]
    /// </summary>
    public class IBMA 
    {
        #region properties

        /// <summary>
        /// uniform layer on the left side
        /// </summary>
        public UniformLayer LayerLeft { get; set; }

        /// <summary>
        /// uniform layer on the right side
        /// </summary>
        public UniformLayer LayerRight { get; set; }

        /// <summary>
        /// coefficients in the homogeneous medium
        /// on the left side
        /// [currently 2-element only]
        /// </summary>
        public VectorZ CLeft { get; set; }

        /// <summary>
        /// coefficients in the homogeneous medium
        /// on the right side
        /// [currently 2-element only]
        /// </summary>
        public VectorZ CRight { get; set; }

        /// <summary>
        /// forward field propagation operator
        /// from left to right
        /// </summary>
        public Func<VectorZ, VectorZ> PForward { get; set; }

        /// <summary>
        /// backward field propagation operator
        /// from right to left
        /// </summary>
        public Func<VectorZ, VectorZ> PBackward { get; set; }

        /// <summary>
        /// ratio used for mixing the test and true solutions
        /// </summary>
        public double MixRatio { get; set; } = 0.75;


        public VectorZ? CLP { get; set; }
        public VectorZ? CLM { get; set; }
        public VectorZ? CRP { get; set; }
        public VectorZ? CRM { get; set; }


        #endregion
        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nLeft"></param>
        /// <param name="nRight"></param>
        /// <param name="cLeft"></param>
        /// <param name="cRight"></param>
        /// <param name="pForward"></param>
        /// <param name="pBackward"></param>
        public IBMA(Complex nLeft, Complex nRight,
            VectorZ? cLeft = null, VectorZ? cRight = null,
            Func<VectorZ, VectorZ>? pForward = null,
            Func<VectorZ, VectorZ>? pBackward = null) 
        {
            // defines uniform layers
            LayerLeft = new(n: nLeft);
            LayerRight = new(n: nRight);

            // defaults parameters
            CLeft = cLeft ?? new VectorZ(count: 2);
            CLeft[0] = 1.0; // input from the left
            CLeft[1] = 0.0; // reflection to the left
            CRight = cRight ?? new VectorZ(count: 2);
            CRight[0] = 0.0; // transmission to the right
            CRight[1] = 0.0; // input from the right

            // defaults field propagation operators
            PForward = pForward ?? ((v) => v);
            PBackward = pBackward ?? ((v) => v);
        }

        #endregion
        #region methods


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavelength"></param>
        /// <param name="polarization"></param>
        /// <param name="cInP"></param>
        /// <param name="cInM"></param>
        /// <param name="kx"></param>
        /// <param name="w"></param>
        /// <param name="n"></param>
        public void Iteration(double wavelength,
            InPlanePolMode polarization,
            Complex cInP, Complex cInM,
            double kx = 0.0,
            double w = 0.75,
            long n = 19)
        {
            // parameters
            double k0 = 2.0 * Math.PI / wavelength;
            double nx = kx / k0;
            CLP = new(count: n);
            CLM = new(count: n);
            CRP = new(count: n);
            CRM = new(count: n);

            // w-matrix on the left
            (_, Complex wL1, Complex wL2) = LayerLeft.ComputeInPlaneModes(wavelength, nx, polarization);
            MatrixZ wL = new(rows: 2, cols: 2);
            wL[0, 0] = wL1; wL[0, 1] = wL1;
            wL[1, 0] = wL2; wL[1, 1] = -wL2;

            // w-matrix on the right
            (_, Complex wR1, Complex wR2) = LayerRight.ComputeInPlaneModes(wavelength, nx, polarization);
            MatrixZ wR = new(rows: 2, cols: 2);
            wR[0, 0] = wR1; wR[0, 1] = wR1;
            wR[1, 0] = wR2; wR[1, 1] = -wR2;

            // loop
            for (long i = 0; i < n; i++)
            {
                VectorZ f;

                // left: coefficient to field
                f = LinAlg.Dot(wL, CLeft);
                // field propagation to right
                f = PForward(f);
                // right: field to coefficient
                CRight = LinAlg.LinearSolve(wR, f);
                // value mixing on the right
                CRight[1] = (w * CRight[1] + cInM) / (1.0 + w);

                // right: coefficient to field
                f = LinAlg.Dot(wR, CRight);
                // field propagation to left
                f = PBackward(f);
                // left: field to coefficient
                CLeft = LinAlg.LinearSolve(wL, f);
                // value mixing on the left
                CLeft[0] = (w * CLeft[0] + cInP) / (1.0 + w);

                // stores intermediate results
                CLP[i] = CLeft[0];
                CLM[i] = CLeft[1];
                CRP[i] = CRight[0];
                CRM[i] = CRight[1];
            }

        }

        #endregion
    }


    /// <summary>
    /// Iterative Boundary-value Algorithm
    /// </summary>
    public class IBVA1D
    {
        #region properties

        /// <summary>
        /// uniform layer on the left side
        /// </summary>
        public UniformLayer LayerLeft { get; set; }

        /// <summary>
        /// uniform layer on the right side
        /// </summary>
        public UniformLayer LayerRight { get; set; }

        /// <summary>
        /// coefficients in the homogeneous medium
        /// on the left side
        /// </summary>
        public VectorZ InputLeft { get; set; }

        /// <summary>
        /// coefficients in the homogeneous medium
        /// on the right side
        /// </summary>
        public VectorZ InputRight { get; set; }

        /// <summary>
        /// forward field propagation operator
        /// from left to right
        /// </summary>
        public Func<VectorZ, VectorZ> ForwardProp { get; set; }

        /// <summary>
        /// backward field propagation operator
        /// from right to left
        /// </summary>
        public Func<VectorZ, VectorZ> BackwardProp { get; set; }

        /// <summary>
        /// ratio used for mixing the test and true solutions
        /// </summary>
        public double MixRatio { get; set; } = 0.75;

        /// <summary>
        /// save the positive coefficient on the left side
        /// </summary>
        public List<VectorZ> Tleft { get; set; }

        /// <summary>
        /// save the minus coefficient on the left side 
        /// </summary>
        public List<VectorZ> Rleft { get; set; }

        /// <summary>
        /// save the positive coefficient on the right side 
        /// </summary>
        public List<VectorZ> Tright { get; set; }

        /// <summary>
        /// save the minus coefficient on the right side  
        /// </summary>
        public List<VectorZ> Rright { get; set; }

        /// <summary>
        /// sampling grid in k-domain
        /// </summary>
        public GridInfo1D GridK { get; set; }

        /// <summary>
        /// if show the progress of the IBVA
        /// </summary>
        public bool IfShowProgress { get; set; }

        #endregion
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layerLeft"> uniform layer on the left side </param>
        /// <param name="layerRight"> uniform layer on the right side </param>
        /// <param name="inputLeft"> initial coefficients on the left side with positive coefficent in front and minus behind </param>
        /// <param name="inputRight"> initial coefficients on the right side with positive coefficent in front and minus behind </param>
        /// <param name="forwardPropagation"> forward propagate method </param>
        /// <param name="backwardPropagation"> backward propagate method </param>
        /// <param name="ifShowProgress"> if show the progress of IBVA </param>
        public IBVA1D(
            UniformLayer layerLeft, UniformLayer layerRight,
            VectorZ inputLeft, VectorZ inputRight,
            Func<VectorZ, VectorZ> forwardPropagation,
            Func<VectorZ, VectorZ> backwardPropagation,
            bool ifShowProgress = false)
        {
            // define uniform layers
            LayerLeft  = layerLeft;
            LayerRight = layerRight;

            // initialize the coefficients on each side
            InputLeft = inputLeft;
            InputRight = inputRight;

            // define field propagation operators
            ForwardProp = forwardPropagation;
            BackwardProp = backwardPropagation;

            // define coefficients lists of left side
            Tleft = new List<VectorZ>();
            Rleft = new List<VectorZ>();

            // define coefficients lists of right side
            Tright = new List<VectorZ>();
            Rright = new List<VectorZ>();

            //if show the progress of IBVA
            IfShowProgress = ifShowProgress;
        }

        #endregion
        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavelength"> wavelength of the incident emfield </param>
        /// <param name="polarization"> polarization of the emfield </param>
        /// <param name="gridK"> sample grid in k-domain </param>
        /// <param name="mixRatio"> mixing parameter of each side </param>
        /// <param name="iterations"> how many iterations the IBVA takes </param>
        public void Iteration(
            double wavelength,
            InPlanePolMode polarization,
            GridInfo1D gridK,
            double mixRatio = 0.75,
            long iterations = 12)
        {
            // initialize the proporties
            double k0 = 2 * Math.PI / wavelength;
            GridK = gridK;
            MixRatio = mixRatio;

            // paramters for saving the current coeffients calculated by IBVA
            VectorZ clp = new VectorZ(count: GridK.Count);
            VectorZ clm = new VectorZ(count: GridK.Count);
            VectorZ crp = new VectorZ(count: GridK.Count);
            VectorZ crm = new VectorZ(count: GridK.Count);
            // paramters for saving the current emfield
            VectorZ EMfield = new VectorZ(count: 2 * GridK.Count);

            // get emfield from the coefficients on the left side for each kx
            Action<long> Converting = j =>
            {
                var kx = GridK.GetCoordinate(j);
                var nx = kx / k0;
                
                // construct coefficients vector for each kx 
                var cL = new VectorZ(count: 2);
                cL[0] = InputLeft[j]; cL[1] = InputLeft[j + GridK.Count];

                // get the emfield
                VectorZ f = LayerLeft.ConvertingToField1D(cL, wavelength, nx, polarization);
                EMfield[j] = f[0]; EMfield[j + GridK.Count] = f[1];
            };
            Loop1D loop0 = new Loop1D(Converting, 0, GridK.Count);
            loop0.Evaluate(LoopMode.Parallel);

            // loop
            for (long i = 0; i < iterations; i++)
            {
                EMfield = ForwardProp(EMfield);

                // mix the field on the right side
                (crp, crm, EMfield) = MixingRight(
                       wavelength: wavelength,
                       EMfield: EMfield,
                       polarization: polarization);
                Tright.Add(crp); Rright.Add(crm);

                EMfield = BackwardProp(EMfield);

                // mix the field on the left side
                (clp, clm, EMfield) = MixingLeft(
                       wavelength: wavelength,
                       EMfield: EMfield,
                       polarization: polarization);
                Tleft.Add(clp); Rleft.Add(clm);
                if (IfShowProgress)
                {
                    double ii = i + 1;
                    double p = ii / iterations;
                    Printer.Write($"... current progress of the IBVM solver: {p * 100}%;");
                }
            }
        }

        /// <summary>
        /// mixing oparetor in the left side
        /// </summary>
        /// <param name="wavelength"> wavelength of the incident emfield </param>
        /// <param name="EMfield"> the emfield with E in front and H behind </param>
        /// <param name="polarization"> polarization of the emfield </param>
        /// <returns></returns>
        protected (VectorZ, VectorZ, VectorZ) MixingLeft(
                  double wavelength, 
                  VectorZ EMfield, 
                  InPlanePolMode polarization)
        {
            // calculate k0
            double k0 = 2 * Math.PI / wavelength;

            // vector for saving the current result of the coefficients on the left side
            VectorZ clp = new VectorZ(count: GridK.Count);
            VectorZ clm = new VectorZ(count: GridK.Count);

            //mixing on the left side
            Action<long>    mixing = i =>
            {
                var kx = GridK.GetCoordinate(i);
                var nx = kx / k0;

                // get coefficients from E and H on left side
                (_, Complex wL1, Complex wL2) = LayerLeft.ComputeInPlaneModes(wavelength, nx, polarization);
                MatrixZ wL = new(rows: 2, cols: 2);
                wL[0, 0] = wL1; wL[0, 1] = wL1;
                wL[1, 0] = wL2; wL[1, 1] = -wL2; 
                //consturct a vector saving emfield of each kx
                VectorZ f = new VectorZ(count: 2);
                f[0] = EMfield[i]; f[1] = EMfield[i + GridK.Count];
                //consturct a vector saving coefficients of each kx
                var cL = new VectorZ(count: 2);
               
                // get coefficients from emfield
                cL = LinAlg.LinearSolve(wL, f);
                
                // value mixing on the left
                cL[0] = (MixRatio * cL[0] + InputLeft[i]) / (1.0 +  MixRatio);

                // get emfield from coefficients
                f = LinAlg.Dot(wL, cL);
                EMfield[i] = f[0]; EMfield[i + GridK.Count] = f[1];

                clp[i] = cL[0]; clm[i] = cL[1];
            };
            Loop1D loopL = new Loop1D(mixing, 0, GridK.Count);
            loopL.Evaluate(LoopMode.Parallel);

            // return the coefficients and emfield
            return (clp, clm, EMfield);
        }

        /// <summary>
        /// mixing oparetor in the right side 
        /// </summary>
        /// <param name="wavelength"> wavelength of the incident emfield </param>
        /// <param name="EMfield"> the emfield with E in front and H behind </param>
        /// <param name="polarization"> polarization of the emfield </param>
        /// <returns></returns>
        protected (VectorZ, VectorZ, VectorZ) MixingRight(
                  double wavelength,
                  VectorZ EMfield,
                  InPlanePolMode polarization)
        {
            // k number
            double k0 = 2 * Math.PI / wavelength;

            // empty field coefficient vectors
            VectorZ crp = new VectorZ(count: GridK.Count);
            VectorZ crm = new VectorZ(count: GridK.Count);

            Action<long> mixing = i =>
            {
                var kx = GridK.GetCoordinate(i);
                var nx = kx / k0;

                // get coefficients from E and H on right side
                (_, Complex wR1, Complex wR2) = LayerRight.ComputeInPlaneModes(wavelength, nx, polarization);
                MatrixZ wR = new(rows: 2, cols: 2);
                wR[0, 0] = wR1; wR[0, 1] = wR1;
                wR[1, 0] = wR2; wR[1, 1] = -wR2;
                //consturct a vector saving coefficients of each kx
                VectorZ f = new VectorZ(count: 2);
                f[0] = EMfield[i]; f[1] = EMfield[i + GridK.Count];
                //consturct a vector saving coefficients of each kx
                var cR = new VectorZ(count: 2);
                // get coefficients from emfield
                cR = LinAlg.LinearSolve(wR, f);
                // value mixing on the right
                cR[1] = (MixRatio * cR[1] + InputRight[i + GridK.Count]) / (1.0 + MixRatio);

                // get emfield from coefficients
                f = LinAlg.Dot(wR, cR);
                EMfield[i] = f[0]; EMfield[i + GridK.Count] = f[1];

                crp[i] = cR[0]; crm[i] = cR[1];
            };
            Loop1D loopL = new Loop1D(mixing, 0, GridK.Count);
            loopL.Evaluate(LoopMode.Parallel);

            // return the coefficients and emfield
            return (crp, crm, EMfield);
        }



        #endregion
    }



}
