using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    public class IBVA2D
    {
        #region proporties

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
        public MatrixZ InputLeft { get; set; }

        /// <summary>
        /// coefficients in the homogeneous medium
        /// on the right side
        /// </summary>
        public MatrixZ InputRight { get; set; }

        /// <summary>
        /// forward field propagation operator
        /// from left to right
        /// </summary>
        public Func<MatrixZ, MatrixZ> ForwardProp { get; set; }

        /// <summary>
        /// backward field propagation operator
        /// from right to left
        /// </summary>
        public Func<MatrixZ, MatrixZ> BackwardProp { get; set; }

        /// <summary>
        /// ratio used for mixing the test and true solutions
        /// </summary>
        public double MixRatio { get; set; } = 0.75;

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> TleftX { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> RleftX { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> TrightX { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> RrightX { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> TleftY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> RleftY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> TrightY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<MatrixZ> RrightY { get; set; }

        /// <summary>
        /// sampling grid for k
        /// </summary>
        public GridInfo2D  GridK { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IfShowProgress { get; set; }

        #endregion

        #region constructors

        public IBVA2D(
            UniformLayer layerLeft, UniformLayer layerRight,
            MatrixZ inputLeft, MatrixZ inputRight,
            Func<MatrixZ, MatrixZ> forwardPropagation,
            Func<MatrixZ, MatrixZ> backwardPropagation,
            bool ifShowProgress)
        {
            // define uniform layers
            LayerLeft = layerLeft;
            LayerRight = layerRight;

            // defaults parameters
            InputLeft = inputLeft;
            InputRight = inputRight;

            // defaults field propagation operators
            ForwardProp = forwardPropagation;
            BackwardProp = backwardPropagation;
            IfShowProgress = ifShowProgress;

            // define coefficients lists of left side
            TleftX = new List<MatrixZ>();
            RleftX = new List<MatrixZ>();
            TleftY = new List<MatrixZ>();
            RleftY = new List<MatrixZ>();

            // define coefficients lists of right side
            TrightX = new List<MatrixZ>();
            RrightX = new List<MatrixZ>();
            TrightY = new List<MatrixZ>();
            RrightY = new List<MatrixZ>();
        }

        #endregion
        
        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavelength"></param>
        /// <param name="samplegrid"></param>
        public void Iteration(
            double wavelength,
            GridInfo2D samplegrid,
            double mixRatio = 0.75,
            long iterations = 10)
        {
            GridK = samplegrid;
            MixRatio = mixRatio;
            // parameters
            double k0 = 2 * Math.PI / wavelength;
            MatrixZ clpx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ clmx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crpx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crmx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ clpy = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ clmy = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crpy = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crmy = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ EMfield = new MatrixZ(4 * GridK.Rows, GridK.Cols);

            Parallel.For(0, GridK.Cols, ix =>
            {
                double nx = GridK.GetCoordinateX(ix) / k0;
                Parallel.For(0, GridK.Rows, iy =>
                {
                    double ny = GridK.GetCoordinateY(iy) / k0;

                    //generate coefficients for the left side for every single (kx, ky)
                    VectorZ cL = new VectorZ(4);
                    cL[0] = InputLeft[iy, ix]; cL[1] = InputLeft[iy + GridK.Rows, ix];
                    cL[2] = InputLeft[iy + 2 * GridK.Rows, ix]; ; cL[3] = InputLeft[iy + 3 * GridK.Rows, ix];
                    
                    //converting
                    VectorZ f = LayerLeft.ConvertingToField2D(cL, wavelength, nx, ny);
                    EMfield[iy, ix] = f[0]; EMfield[iy + GridK.Rows, ix] = f[1];
                    EMfield[iy + 2 * GridK.Rows, ix] = f[2]; EMfield[iy + 3 * GridK.Rows, ix] = f[3];
                });
            });

            // loop
            for (long i = 0; i < iterations; i++)
            {
                EMfield = ForwardProp(EMfield);

                // mix the field on the right side
                (crpx, crpy, crmx, crmy, EMfield) = MixingR(
                       wavelength: wavelength,
                       EMfield: EMfield);
                TrightX.Add(crpx); TrightY.Add(crpy); RrightX.Add(crmx); RrightY.Add(crmy);

                EMfield = BackwardProp(EMfield);

                // mix the field on the left side
                (clpx, clpy, clmx, clmy, EMfield) = MixingL(
                       wavelength: wavelength,
                       EMfield: EMfield);
                TleftX.Add(clpx); TleftY.Add(clpy); RleftX.Add(clmx); RleftY.Add(clmy);
                if(IfShowProgress)
                {
                    double ii = i + 1;
                    double p = ii / iterations;
                    Printer.Write($"... current progress of the IBVA solver: {p * 100}%;");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavelength"> the length of light </param>
        /// <param name="EMfield"></param>
        /// <returns></returns>
        protected (MatrixZ clpx, MatrixZ clpy, MatrixZ clmx, MatrixZ clmy, MatrixZ emfiled) MixingL(
                  double wavelength,
                  MatrixZ EMfield)
        {
            // k number
            double k0 = 2 * Math.PI / wavelength;

            // empty field coefficient vectors
            MatrixZ clpx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ clmx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ clpy = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ clmy = new MatrixZ(GridK.Rows, GridK.Cols);

            Parallel.For(0, GridK.Cols, ix =>
            {
                double nx = GridK.GetCoordinateX(ix) / k0;
                Parallel.For(0, GridK.Rows, iy =>
                {
                    double ny = GridK.GetCoordinateY(iy) / k0;
                    (_, MatrixZ w1, MatrixZ w2) = LayerLeft.ComputeConicalModes(wavelength, nx, ny);
                    MatrixZ w = new MatrixZ(4, 4);
                    w[new LongRange(0, 2), new LongRange(0, 2)] = w1;
                    w[new LongRange(0, 2), new LongRange(2, 4)] = w1;
                    w[new LongRange(2, 4), new LongRange(0, 2)] = w2;
                    w[new LongRange(2, 4), new LongRange(2, 4)] = -w2;
                    VectorZ f = new VectorZ(4);
                    f[0] = EMfield[iy, ix]; f[1] = EMfield[iy + GridK.Rows, ix];
                    f[2] = EMfield[iy + 2 * GridK.Rows, ix]; f[3] = EMfield[iy + 3 * GridK.Rows, ix];
                    VectorZ cL = new VectorZ(4);
                    cL = LinAlg.LinearSolve(w, f);
                    cL[0] = (MixRatio * cL[0] + InputLeft[iy, ix]) / (1.0 + MixRatio);
                    cL[1] = (MixRatio * cL[1] + InputLeft[iy + GridK.Rows, ix]) / (1.0 + MixRatio);
                    f = LinAlg.Dot(w, cL);
                    EMfield[iy, ix] = f[0]; EMfield[iy + GridK.Rows, ix] = f[1];
                    EMfield[iy + 2 * GridK.Rows, ix] = f[2]; EMfield[iy + 3 * GridK.Rows, ix] = f[3];
                    clpx[iy, ix] = cL[0]; clpy[iy, ix] = cL[1];
                    clmx[iy, ix] = cL[2]; clmy[iy, ix] = cL[3];
                });
            });
            // return light field
            return (clpx, clpy, clmx, clmy, EMfield);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavelength"></param>
        /// <param name="EMfield"></param>
        /// <returns></returns>
        protected (MatrixZ crpx, MatrixZ crpy, MatrixZ crmx, MatrixZ crmy, MatrixZ emfiled) MixingR(
          double wavelength,
          MatrixZ EMfield)
        {
            // k number
            double k0 = 2 * Math.PI / wavelength;

            // empty field coefficient vectors
            MatrixZ crpx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crmx = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crpy = new MatrixZ(GridK.Rows, GridK.Cols);
            MatrixZ crmy = new MatrixZ(GridK.Rows, GridK.Cols);

            Parallel.For(0, GridK.Cols, ix =>
            {
                double nx = GridK.GetCoordinateX(ix) / k0;
                Parallel.For(0, GridK.Rows, iy =>
                {
                    double ny = GridK.GetCoordinateY(iy) / k0;
                    (_, MatrixZ w1, MatrixZ w2) = LayerRight.ComputeConicalModes(wavelength, nx, ny);
                    MatrixZ w = new MatrixZ(4, 4);
                    w[new LongRange(0, 2), new LongRange(0, 2)] = w1;
                    w[new LongRange(0, 2), new LongRange(2, 4)] = w1;
                    w[new LongRange(2, 4), new LongRange(0, 2)] = w2;
                    w[new LongRange(2, 4), new LongRange(2, 4)] = -w2;
                    VectorZ f = new VectorZ(4);
                    f[0] = EMfield[iy, ix]; f[1] = EMfield[iy + GridK.Rows, ix];
                    f[2] = EMfield[iy + 2 * GridK.Rows, ix]; f[3] = EMfield[iy + 3 * GridK.Rows, ix];
                    VectorZ cR = new VectorZ(4);
                    cR = LinAlg.LinearSolve(w, f);
                    cR[2] = (MixRatio * cR[2] + InputRight[iy + 2 * GridK.Rows, ix]) / (1.0 + MixRatio);
                    cR[3] = (MixRatio * cR[3] + InputRight[iy + 3 * GridK.Rows, ix]) / (1.0 + MixRatio);
                    f = LinAlg.Dot(w, cR);
                    EMfield[iy, ix] = f[0]; EMfield[iy + GridK.Rows, ix] = f[1];
                    EMfield[iy + 2 * GridK.Rows, ix] = f[2]; EMfield[iy + 3 * GridK.Rows, ix] = f[3];
                    crpx[iy, ix] = cR[0]; crpy[iy, ix] = cR[1];
                    crmx[iy, ix] = cR[2]; crmy[iy, ix] = cR[3];
                });
            });
            // return light field
            return (crpx, crpy, crmx, crmy, EMfield);
        }
        #endregion
    }
}
