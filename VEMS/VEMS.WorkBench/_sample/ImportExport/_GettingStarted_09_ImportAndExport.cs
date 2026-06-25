// default sample file path
var path = UserSetting.SampleCodeDir;

// import data from text to vector
var importedVecD = Import.Txt2VectorD(filePath: path+@"\ImportExport\ImportTest_VectorReal.txt");
Printer.Write(importedVecD);
// import data from text to matrix
var importedMatD = Import.Txt2MatrixD(filePath: path+@"\ImportExport\ImportTest_MatrixReal.txt");
Printer.Write(importedMatD);
var importedMatZ =Import.Txt2MatrixZ(filePath: path+@"\ImportExport\ImportTest_MatrixComplex.txt");
Printer.Write(importedMatZ);


// test data for export
var v = VStat.RngGaussian(n: 7, a: 0.5, sigma: 0.1);
var m = VStat.RngUniform(rows: 4, cols: 5);
// step #1: creates a DataExporter
var exporter = new DataExporter(); 
// step #2: converts to string [StringBuilder]
//var t = exporter.ToString(vs: v); // for vector
var t = exporter.ToString(vs: m); // for matrix
// step #3: exports to file
//exporter.ExportToText(content: t, targetPath: path + @"\ImportExport\ExportedVector.txt");
exporter.ExportToText(content: t, targetPath: path + @"\ImportExport\ExportedMatrix.txt");