string ip = "10.0.110.251";
int port = 2024;
Socket node = Network.CreateSocket(ip, port);
Printer.Logging($"Created as a node with {node.LocalEndPoint}");
bool nodeAlive = true;
// starts listening
while (nodeAlive)
{
    node.Listen();
    Socket listener = node.Accept(); //await node.AcceptAsync();    
    if (listener.RemoteEndPoint is IPEndPoint ipe)
        Printer.Logging($"connection accepted from ip: {ipe.Address} port: {ipe.Port}");


    // accepting
    byte[] buffer = new byte[1024];
    int received = listener.Receive(buffer, SocketFlags.None); //await listener.ReceiveAsync(buffer, SocketFlags.None);
    string message = Encoding.UTF8.GetString(buffer, 0, received);
    Printer.Logging($"received message: {message}");


    // acknowledgement for ending
    if (message.IndexOf("<|ACK|>") == 0)
    {
        Printer.Logging($"acknowledgement: \"{message}\" confirmed from remote ...");
        nodeAlive = false;
    }


    if (message.IndexOf("<|RUN|>") == 0)
    {
        Printer.Logging($"run command: \"{message}\" confirmed from remote ...");
        string code = message.Remove(0, 7);
        Printer.Write($"code text: {code}");
        // ...
    }


    if (message.IndexOf("<|RWI|>") == 0)
    {
        Printer.Logging($"run with index command: \"{message}\" confirmed from remote ...");
        int idx = int.Parse(message.Remove(0, 7));
        // call RCWA example
        double de = RCWAIdx(idx);
        Printer.Write($"Diffraction efficiency = {de}");
    }

}

// shuts down the socket
Printer.Logging($"shut down current node with {node.LocalEndPoint}");
node.Shutdown(SocketShutdown.Both);



// RCWA example with index
public static double RCWAIdx(int idx)
{
    // parameters
    var wavelength = 1.06E-6;
    var incAngle = 20.173; // in degree, Littrow configuration
    var nIn = 1.45;
    var nOut = 1.0;
    var nRidge = nIn;
    var nEmbed = nOut;
    var period = wavelength;
    var ridgeWidth = 0.5 * period;
    var ridgeCenter = 0.0;
    var pol = InPlanePolMode.TE;
    // numerical parameters
    var N = 101;
    // parameters variation
    var depthStart = 0.1E-6;
    var depthEnd = 10.0E-6;
    var depths = 31;
    var depthi = (depthEnd - depthStart) / (depths - 1);
    var d = depthStart + idx * depthi;
    // ==== RCWA calculation ====
    var kx0 = 2.0 * Math.PI / wavelength * nIn * Math.Sin(Converter.Degree2Radian(incAngle));
    var rSolver = new RCWA1D(wavelength: wavelength,
        nFront: (w, p) => nIn, null,
        nMiddle: (w, x, p) => GratingRIndex(x, p[0], p[1], p[2], p[3], p[4], p[5]),
        pMiddle: new List<double> { ridgeWidth, ridgeCenter, nRidge, 0.0, nEmbed, 0.0 },
        period: period, thickness: d,
        nBehind: (w, p) => nOut, null,
        mode: pol,
        nKxs: N,
        kx0: kx0,
        layerOverSamp: 1.0,
        useWSVariation: false,
        saveLayerMediaData: true,
        saveLayerModesData: false);
    rSolver.ComputeHalfSMatrix();
    // input => output
    var inputPW = new PlaneWaveXZ(wavelength, nIn * nIn, kx0,
        SignFactor.Positive, pol);
    inputPW.E = 1.0;
    var cIn = new VectorZ(N);
    cIn[(N - 1) / 2] = inputPW.E; // single plane wave at zero spatial frequency
    // computes transmission
    var cOut = rSolver.ComputeTCoefficients(cIn);
    // transmission coefficients at given order
    var order = -1;
    var cOuti = cOut[(N - 1) / 2 + order];
    //Printer.Write($"[{order}] order coefficients: {cOuti.Magnitude}*exp({cOuti.Phase})");
    var outputPW = new PlaneWaveXZ(wavelength, nOut * nOut, kx0 + order * 2.0 * Math.PI / period,
        SignFactor.Positive, pol);
    outputPW.E = cOuti;
    var eta = outputPW.ComputeSz() / inputPW.ComputeSz();
    return eta;
}


// ================================
// grating layer epsilon definition
public static Complex GratingRIndex(double x,
    double ridgeWidth,
    double ridgeCenter,
    double nInnerRe, double nInnerIm,
    double nOuterRe, double nOuterIm)
{
    if (Math.Abs(x - ridgeCenter) <= 0.5 * ridgeWidth)
        return new Complex(nInnerRe, nInnerIm);
    else
        return new Complex(nOuterRe, nOuterIm);
}