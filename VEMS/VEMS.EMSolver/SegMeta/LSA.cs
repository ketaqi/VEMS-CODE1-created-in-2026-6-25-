using System.Numerics;
using VEMS.MathCore;

namespace VEMS.EMSolver
{
    ///// <summary>
    ///// local segmented method(LSA)
    ///// on the basement of RCWA / FMM
    ///// </summary>
    //public class LSA
    //{
    //    //#region properties
    //    ///// <summary>
    //    ///// wavelength in vacuum
    //    ///// </summary>
    //    //public double Wavelength { get; set; }

    //    ///// <summary>
    //    ///// uniform layer in front 
    //    ///// </summary>
    //    //public UniformLayer? LayerFront { get; set; }

    //    ///// <summary>
    //    ///// 1D-periodic layer in the middle
    //    ///// </summary>
    //    //public Periodic1DLayer? LayerMiddle { get; set; }

    //    //// for future development
    //    ////public List<Periodic1DLayer> LayersMiddle { get; set; }

    //    ///// <summary>
    //    ///// uniform layer behind
    //    ///// </summary>
    //    //public UniformLayer? LayerBehind { get; set; }

    //    ///// <summary>
    //    ///// in-plane polarization mode option
    //    ///// </summary>
    //    //public InPlanePolMode PolMode { get; set; }
    //    ///// <summary>
    //    ///// number of spatial frequencies for 
    //    ///// E/H-field decomposition
    //    ///// </summary>
    //    //public long NKxs { get; set; }
    //    ///// <summary>
    //    ///// oversampling factor for the layer 
    //    ///// in the middle
    //    ///// </summary>
    //    //public double LayerOverSamp { get; set; }
    //    //#endregion

    //    #region constructor
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="wavelength"></param>
    //    /// <param name="nFront"></param>
    //    /// <param name="pFront"></param>
    //    /// <param name="diameter"></param>
    //    /// <param name="nEmbed"></param>
    //    /// <param name="nPillar"></param>
    //    /// <param name="uniPeriod"></param>
    //    /// <param name="thickness"></param>
    //    /// <param name="nBehind"></param>
    //    /// <param name="pBehind"></param>
    //    /// <param name="mode"></param>
    //    /// <param name="nKxs"></param>
    //    /// <param name="wIllumination"></param>
    //    /// <param name="edgeRatio"></param>
    //    /// <param name="dComputing"></param>
    //    /// <param name="Ein"></param>
    //    /// <param name="kx0"></param>
    //    /// <param name="layerOverSamp"></param>
    //    /// <param name="useWSVariation"></param>
    //    /// <param name="saveLayerMediaData"></param>
    //    /// <param name="saveSMatrixData"></param>
    //    /// <returns></returns>
    //    public static (VectorZ, VectorZ) LSA1D(double wavelength,
    //        Func<double, List<double>, Complex> nFront, List<double> pFront,
    //        VectorD diameter, double nEmbed, double nPillar,
    //        double uniPeriod, double thickness,
    //        Func<double, List<double>, Complex> nBehind, List<double> pBehind,
    //        InPlanePolMode mode,
    //        long nKxs,
    //        double wIllumination,
    //        double edgeRatio,
    //        double dComputing,
    //        VectorZ Ein,
    //        double kx0 = 0.0,
    //        double layerOverSamp = 1.0,
    //        bool useWSVariation = false,
    //        bool saveLayerMediaData = true,
    //        bool saveSMatrixData = false)
    //    {
    //        // front
    //        Complex eFront(double w) => Complex.Pow(nFront.Invoke(w, pFront), 2.0);
    //        UniformLayer layerFront = new(eFront);
    //        // behind
    //        Complex eBehind(double w) => Complex.Pow(nBehind.Invoke(w, pBehind), 2.0);
    //        UniformLayer layerBehind = new(eBehind);
    //        // generate structure from diameter
    //        (var structureGrid, var eps) = GenerateEps(diameter, uniPeriod, nEmbed, nPillar);
    //        //// import epsilon
    //        //var structureGrid = Import.Txt2VectorD(filePath + "790_5_Grid_select.txt");
    //        //var eps_real = Import.Txt2VectorD(filePath + "790_5_Eps_Re_select.txt");
    //        //var eps = new VectorZ(eps_real);

    //        var N = Ein.Count;
    //        // grid infomation of epsilon (without oversampling here)
    //        long n = 2 * N - 1; // sampling number for the whole structure.
    //        double totalSize = structureGrid[^1] - structureGrid[0];//diameter.Count * uniPeriod;
    //        double spacing = dComputing / n;
    //        Printer.Write($"total range = {totalSize * 1E6}[um]");
    //        Printer.Write($"computing length = {dComputing * 1E6}[um]");
    //        Printer.Write($"spacing = {spacing * 1E9}[nm]");
    //        if (layerOverSamp > 1.0)
    //        {
    //            var nOverSamp = (long)Math.Ceiling(layerOverSamp * n);
    //            if (nOverSamp % 2 == 0)
    //            { nOverSamp += 1; }
    //            var spacingOver = dComputing / nOverSamp;
    //            Printer.Write($"over sampling spacing = {spacingOver * 1.0E9}[nm]");
    //        }

    //        //incident beam (x-domain)
    //        GridInfo1D gridEin = new(N, totalSize / N);

    //        // initialization
    //        double k0 = 2.0 * Math.PI / wavelength;

    //        // ------------------------------- local simulation ------------------------------

    //        // compute the central positions of local illumination
    //        long n_center = (long)Math.Ceiling(totalSize / wIllumination);
    //        GridInfo1D centralPoints = new(n_center, wIllumination);

    //        // enlarge input field with additional ZERO values for decomposing
    //        (var inputEnlarge, LongRange inputIndex) = EnlargeField(new Grid1DCplxData(Ein, gridEin),
    //            centralPoints, nKxs, new Complex(0.0, 0.0));

    //        // enlarge eps with Air for decomposing
    //        (var positionEnlarge, var epsEnlarge) = EnlargeEps(dComputing, structureGrid, eps, new Complex(1.0, 0.0));

    //        // save transmission and reflection
    //        var transTotal = new Grid1DCplxData(new VectorZ(inputEnlarge.GridInfo.Count, 0.0), inputEnlarge.GridInfo);
    //        var reflTotal = new Grid1DCplxData(new VectorZ(inputEnlarge.GridInfo.Count, 0.0), inputEnlarge.GridInfo);

    //        #region local s-matrix
    //        Printer.Write($"{centralPoints.Count} segmented regions in total");
    //        // timer
    //        var sw = new System.Diagnostics.Stopwatch();
    //        Printer.Write(" => timer starts ...");
    //        sw.Start();
    //        // loop to calculate decomposed field
    //        for (long i = 0; i < centralPoints.Count; i++)
    //        //long i = 30;
    //        {
    //            if (i != centralPoints.Count - 1)
    //                Console.Write(i.ToString() + " ");
    //            else
    //                Printer.Write(i);

    //            var n_cal = 2 * nKxs - 1;
    //            // decompose epsilon
    //            (var segPosition, var segEps) = DecompEpsilon(centralPoints.GetCoordinate(i), dComputing, positionEnlarge, epsEnlarge);
    //            // define middle layer
    //            var SampEpsilon = PillarEpsilon(wavelength, segPosition, segEps);
    //            //var layerMiddle = new Periodic1DLayer(dComputing, SampEpsilon, thickness);
    //            var layerMiddle = new Periodic1DLayer(dComputing, new Layer1DMedium(n: (w,x)=>1.0), thickness);
    //            // !!!

    //            // call the S-matrix
    //            (var s11, var s21) = SMatrix.HalfSMatrix(wavelength,
    //                layerFront, layerMiddle, layerBehind,
    //                mode: mode,
    //                nKxs: nKxs,
    //                kx0: kx0,
    //                mediumOverSampX: layerOverSamp,
    //                useWSvariation: useWSVariation,
    //                saveLayerMediaData: saveLayerMediaData); // with W1/2 symmetry, recommand W=>t=>S

    //            // if save s-matrix, export to text file...
    //            if (saveSMatrixData)
    //            {
    //                // export file path???
    //                //Export.ExportMatrix2Text(s11,filePath+$"s11-{i}.txt",digits:16); 
    //            }

    //            // decompose input field to local field
    //            var EInLocal = DecomposedField(inputEnlarge, centralPoints.GetCoordinate(i), wIllumination,
    //                edgeRatio, dComputing, s11.Cols);

    //            // transform input field to k-domain
    //            Transform.FFT(EInLocal, FFTOption.Forward);

    //            //trans & refl
    //            var transmission = LinAlg.Dot(s11, EInLocal.Values);
    //            var reflection = LinAlg.Dot(s21, EInLocal.Values);

    //            //inverse Fourier Transform
    //            Transform.FFT(transmission, EInLocal.GridInfo, FFTOption.Backward);

    //            // shift the zero point of transmission field back to central point
    //            var transShift = new Grid1DCplxData(transmission, new GridInfo1D(EInLocal.GridInfo.Count,
    //                EInLocal.GridInfo.Start + centralPoints.GetCoordinate(i), EInLocal.GridInfo.Spacing));
    //            var reflShift = new Grid1DCplxData(reflection, new GridInfo1D(EInLocal.GridInfo.Count,
    //                EInLocal.GridInfo.Start + centralPoints.GetCoordinate(i), EInLocal.GridInfo.Spacing));

    //            // coherent overlap all the result
    //            var interpT = Interpolation.Linear(transShift.Values, transShift.GridInfo, transTotal.GridInfo);
    //            var interpR = Interpolation.Linear(reflShift.Values, reflShift.GridInfo, reflTotal.GridInfo);
    //            transTotal.Values += interpT;
    //            reflTotal.Values += interpR;
    //        }
    //        // cut off the enlarged part
    //        var transFinal = new Grid1DCplxData(transTotal.Values[inputIndex], gridEin);

    //        // timer
    //        sw.Stop();
    //        Printer.Write(" => timer stopped");
    //        Printer.Write($" ==> kernel time cost = {sw.Elapsed.TotalMilliseconds / 1e3} [s]");
    //        #endregion
    //        return (transFinal.Values, reflTotal.Values);
    //    }
    //    #endregion

    //    #region kernel methods
    //    /// <summary>
    //    /// generate grid position and epsilon from diameter
    //    /// </summary>
    //    /// <param name="diameter">nano-structure diameter</param>
    //    /// <param name="period">unit period of the nano-structure</param>
    //    /// <param name="nEmbed">refractive index n of embedment</param>
    //    /// <param name="nPillar">refractive index n of pillar</param>
    //    /// <returns>(grid[vecD],epsilon[vecZ])</returns>
    //    private static (VectorD, VectorZ) GenerateEps(VectorD diameter,
    //        double period, double nEmbed, double nPillar)
    //    {
    //        // generate center grid
    //        var countPillar = diameter.Count;
    //        var grid = new VectorD(2 * countPillar + 2, 0.0);
    //        var epsilon = new VectorZ(2 * countPillar + 1, nEmbed);

    //        grid[0] = -countPillar * period / 2.0;
    //        grid[^1] = -grid[0];
    //        epsilon[^1] = 1.0;
    //        // calculate the center position of each unit
    //        var gridSamp = new GridInfo1D(countPillar, period);
    //        // loop over all pillar width and epsilon
    //        for (long i = 0; i < countPillar; i++)
    //        {
    //            double xCenter = gridSamp.GetCoordinate(i);
    //            grid[2 * i + 1] = xCenter - diameter[i] / 2.0;
    //            grid[2 * i + 2] = xCenter + diameter[i] / 2.0;
    //            epsilon[2 * i] = 1.0;
    //            epsilon[2 * i + 1] = nPillar * nPillar;
    //        }

    //        return (grid, epsilon);
    //    }

    //    /// <summary>
    //    /// Enlarge the field / epsilon with given complex value.
    //    /// </summary>
    //    /// <param name="field"> field to be enlarged </param>
    //    /// <param name="centerInfo"> decomposed central point information </param>
    //    /// <param name="n_cal"> calculating sampling counts </param>
    //    /// <param name="enlargeExample"> additional complex value </param>
    //    /// <returns> enlarged field </returns>
    //    private static (Grid1DCplxData, LongRange) EnlargeField(Grid1DCplxData field, GridInfo1D centerInfo,
    //        long n_cal, Complex enlargeExample)
    //    {
    //        // get the central point index
    //        long centerIndexFront = InRangeIndex(field.GridInfo, centerInfo.Start);
    //        // calculate enlarge start index
    //        long startIndex = centerIndexFront - n_cal / 2;
    //        // get the central point index
    //        long centerIndexBehind = InRangeIndex(field.GridInfo, centerInfo.End);
    //        // calculate enlarge end index
    //        long endIndex = centerIndexBehind + n_cal / 2;


    //        if (startIndex >= 0 && endIndex < field.GridInfo.Count)
    //            return (field, new LongRange(0, field.GridInfo.Count));
    //        else
    //        {

    //            // get absolute of the start index
    //            long startAbs = Math.Abs(startIndex);
    //            // generate enlarged input field
    //            var sampEnlarge = new GridInfo1D(startAbs + endIndex + 1,
    //                field.GridInfo.Start - field.GridInfo.Spacing * startAbs, field.GridInfo.Spacing);
    //            // enlarge vectorZ with additional ZERO values
    //            var inputEnlarge = new VectorZ(sampEnlarge.Count, enlargeExample);

    //            // index range
    //            var indexRange = new LongRange(startAbs, startAbs + field.GridInfo.Count);
    //            // copy the original input field
    //            inputEnlarge[indexRange] = field.Values;

    //            var enlargeField = new Grid1DCplxData(inputEnlarge, sampEnlarge);
    //            return (enlargeField, indexRange);
    //        }
    //    }

    //    /// <summary>
    //    /// Enlarge the epsilon with given material.
    //    /// </summary>
    //    /// <param name="computeLength">computing length</param>
    //    /// <param name="position">grid position of epsilon</param>
    //    /// <param name="eps">[vecZ] epsilon distribution</param>
    //    /// <param name="enlargeExample">given material</param>
    //    /// <returns> ([vecD] grid position of epsilon,[vecZ] enlarged epsilon) </returns>
    //    private static (VectorD, VectorZ) EnlargeEps(double computeLength, VectorD position, VectorZ eps, Complex enlargeExample)
    //    {
    //        // generate the enlarged position
    //        var positionEn = new VectorD(position.Count + 2);
    //        var range = new LongRange(1, position.Count + 1);
    //        positionEn[0] = position[0] - computeLength;
    //        positionEn[^1] = position[^1] + computeLength;
    //        positionEn[range] = position;

    //        // generate the enlarged epsilon
    //        var epsEn = new VectorZ(eps.Count + 2);
    //        var rangeEps = new LongRange(1, eps.Count + 2);
    //        epsEn[0] = enlargeExample;
    //        epsEn[^1] = enlargeExample;
    //        epsEn[rangeEps] = eps;

    //        return (positionEn, epsEn);
    //    }

    //    /// <summary>
    //    /// find the index of given position in the sampling range 
    //    /// </summary>
    //    /// <param name="samp"> sampling information </param>
    //    /// <param name="a"> position </param>
    //    /// <returns> the index </returns>
    //    public static long InRangeIndex(GridInfo1D samp, double a)
    //    {
    //        long index = 0;
    //        if (a < samp.Start || a > samp.End)
    //            Printer.Warning("point out of range!");
    //        else
    //        {
    //            // get the index floor position
    //            long n = (long)Math.Floor((a - samp.Start) / samp.Spacing);
    //            double x = samp.GetCoordinate(n);
    //            double x1 = samp.GetCoordinate(n + 1);
    //            // find to which index the point is closer
    //            index = Math.Abs(x - a) <= Math.Abs(x1 - a) ? n : (n + 1);
    //        }
    //        return index;
    //    }

    //    /// <summary>
    //    /// decompose nano-structure into segmented grid and epsilon
    //    /// </summary>
    //    /// <param name="centralPoint"> central position of the decomposed field </param>
    //    /// <param name="computeLength">compute length</param>
    //    /// <param name="grid">epsilon changed position</param>
    //    /// <param name="eps">epsilon distribution in layerMiddle</param>
    //    /// <returns>(grid[vecD],epsilon[vecZ])</returns>
    //    /// <exception cref="Exception"></exception>
    //    private static (VectorD, VectorZ) DecompEpsilon(double centralPoint, double computeLength,
    //        VectorD grid, VectorZ eps)
    //    {
    //        long startIndex = 0;
    //        long endIndex = grid.Count;
    //        double start = centralPoint - computeLength / 2.0;
    //        double end = centralPoint + computeLength / 2.0;
    //        if (grid[0] >= start)
    //        { throw new Exception("start point out of range!"); }
    //        // get the start point index
    //        for (long i = 0; i < grid.Count; i++)
    //        {
    //            if (grid[i] > start)
    //            { startIndex = i - 1; break; }
    //        }
    //        // get the end point index
    //        for (long i = startIndex; i < grid.Count; i++)
    //        {
    //            if (grid[i] >= end)
    //            { endIndex = i; break; }
    //        }

    //        // segment the structure from start to the end index
    //        var rangePosition = new LongRange(startIndex, endIndex + 1);
    //        var positionSeg = grid[rangePosition];
    //        var rangeEpsilon = new LongRange(startIndex, endIndex);
    //        var epsilonSeg = eps[rangeEpsilon];
    //        // set the start and end position
    //        positionSeg[0] = start;
    //        positionSeg[^1] = end;
    //        // shift to the zero point
    //        positionSeg -= centralPoint;

    //        return (positionSeg, epsilonSeg);
    //    }

    //    /// <summary>
    //    /// convert from piecewise to grid samplings of epsilon
    //    /// </summary>
    //    /// <param name="wavelength">wavelength</param>
    //    /// <param name="grid">position of epsilon</param>
    //    /// <param name="epsilon">epsilon valus</param>
    //    /// <returns>grid sampling epsilon</returns>
    //    private static Func<double, GridInfo1D, VectorZ> PillarEpsilon(double wavelength,
    //        VectorD grid, VectorZ epsilon)
    //    {
    //        // middle
    //        VectorZ eps(double w, GridInfo1D g)
    //        {
    //            // change to grid sampling
    //            var sampEps = new VectorZ(g.Count, 0.0);
    //            for (int i = 0; i < g.Count; i++)
    //            {
    //                double x = g.GetCoordinate(i);
    //                for (int j = 1; j < grid.Count; j++)
    //                {
    //                    if (grid[j - 1] <= x && x < grid[j])
    //                        sampEps[i] = epsilon[j - 1];
    //                }
    //            }
    //            return sampEps;
    //        }
    //        return eps;
    //    }

    //    /// <summary>
    //    /// generate the given index decomposed input field with given width and smooth edge.  
    //    /// </summary>
    //    /// <param name="field"> input field values </param>
    //    /// <param name="centralPoint"> central position of the decomposed field </param>
    //    /// <param name="width"> local illuminatin width w </param>
    //    /// <param name="edgeRatio"> smooth edge rate of illumination </param>
    //    /// <param name="computeLength"></param>
    //    /// <param name="N_cal"> sampling counts of the decomposed field </param>
    //    /// <returns> the decomposed local input field </returns>
    //    private static Grid1DCplxData DecomposedField(Grid1DCplxData field,
    //        double centralPoint, double width, double edgeRatio, double computeLength, long N_cal)
    //    {
    //        // constructs rectangular function
    //        var r = new Samp1DRealFunc(f: Function1D.CosEdgeRectangle,
    //        p: new List<double> { width, edgeRatio * width, centralPoint, 1.0 });
    //        // samples the functions on uniform grid
    //        var sr = r.Sample(field.GridInfo);
    //        // decompose input field with cosine edge rectangle function.
    //        var srField = sr * field.Values;

    //        // shift the centralPoint to zero point
    //        var gridShift = new GridInfo1D(field.GridInfo.Count, field.GridInfo.Start - centralPoint, field.GridInfo.Spacing);
    //        // constructs the target grid
    //        var grid = new GridInfo1D(N_cal, computeLength / N_cal);
    //        // interpolate the field
    //        var localEin = Interpolation.Linear(srField, gridShift, grid);

    //        //    var figure2 = VFrame.CreateFrame();
    //        //    VFrame.AddToFrame(figure2, field.Values, new GridInfo1D(field.Grid.Count, 0, 1), plotColor: Options.PlotColor.SteelBlue, lineWidth: 2);
    //        //    VFrame.AddToFrame(figure2, srField, new GridInfo1D(field.Grid.Count, 0, 1), plotColor: Options.PlotColor.Brown, lineWidth: 2);
    //        //    VFrame.AddToFrame(figure2, localEin, new GridInfo1D(field.Grid.Count, 0, 1), plotColor: Options.PlotColor.Red, lineWidth: 2);
    //        //    VFrame.RefreshShow(figure2);

    //        return new Grid1DCplxData(localEin, grid);
    //    }
    //    #endregion
    //}
}
