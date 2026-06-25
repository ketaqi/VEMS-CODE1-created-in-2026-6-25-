// target file path
var path = UserSetting.SampleCodeDir;
string exptPath = path + @"\ImportExport\ExportedBigMatrix.txt";


// generate big matrix for export
var n = 1001;
var xRe = VStat.RngUniform(rows: n, cols: n);
var xIm = VStat.RngUniform(rows: n, cols: n);
var x = VMath.Construct(xRe, xIm);


// step #1: creates data exporter
var exporter = new DataExporter(digits: 16);
Printer.Write($"timer starts ...");
var sw = Stopwatch.StartNew();
// step #2: converts to string
var content = exporter.ToString(x, loopMode: LoopMode.Parallel); // enable parallel loop
sw.Stop();
Printer.Write($"conversion to string: {sw.ElapsedMilliseconds} [ms]");
sw.Restart();
// step #3: export to file
exporter.ExportToText(content, exptPath);
sw.Stop();
Printer.Write($"write to file: {sw.ElapsedMilliseconds} [ms]");