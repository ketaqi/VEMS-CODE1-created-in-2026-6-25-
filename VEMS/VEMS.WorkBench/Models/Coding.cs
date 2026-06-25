using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System;
using VEMS.MathCore;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.IO;

namespace VEMS.WorkBench
{
    /// <summary>
    /// code diagnostic class
    /// </summary>
    public class Coding
    {

        /// <summary>
        /// creates a RoslynHost with default references
        /// for the VEMS project
        /// </summary>
        /// <returns> generated RoslynHost </returns>
        internal static RoslynHost CreateHost()
            => new(additionalAssemblies: new[]{
                Assembly.Load("RoslynPad.Roslyn.Windows"),
                Assembly.Load("RoslynPad.Editor.Windows") },
                references: RoslynHostReferences.NamespaceDefault.With(
                    typeNamespaceImports: new[]{
                        typeof(object),
                        typeof(System.Text.RegularExpressions.Regex),
                        typeof(Enumerable),
                        typeof(Console),
                        typeof(LinAlg),
                        typeof(WMathCore.WMatrixD),
                        typeof(System.Numerics.Complex),
                        typeof(EMSolver.EMField),
                        typeof(Plot.Figure),
                        typeof(Printer),
                        typeof(FigTest),
                        typeof(VFrame),
                        typeof(RayTrace.RayBase),
                        typeof(Parallel.Master),
                        typeof(System.Net.Sockets.Socket),
                        typeof(System.Net.IPEndPoint),
                        typeof(System.Threading.Tasks.Parallel),
                        typeof(System.Diagnostics.Stopwatch) }));

        /// <summary>
        /// makes script from given code text
        /// </summary>
        /// <param name="codeText"> input text containing the code </param>
        /// <param name="host"> Roslyn host used for the script generation </param>
        /// <returns> result script </returns>
        internal static Script<object> MakeScript(string codeText,
            RoslynHost? host = null)
        {
            host ??= CreateHost();
            return CSharpScript.Create(code: codeText, 
                options: ScriptOptions.Default.WithReferences(host.DefaultReferences).WithImports(host.DefaultImports));
        }

        /// <summary>
        /// checks for error in a script
        /// </summary>
        /// <param name="scr"> script to check </param>
        /// <param name="logFullDiagnInfo"> whether to log full diagnostic info </param>
        /// <param name="errorPrompt"> error prompt for display </param>
        /// <returns> diagnostic result </returns>
        public static (bool, string) CheckError(Script<object> scr,
            bool logFullDiagnInfo = false,
            string errorPrompt = "[Error]")
        {
            ImmutableArray<Diagnostic> dgn = scr.Compile();

            // initializes the flag
            bool hasError = false;
            string errorInfo = string.Empty;
            for (int i = 0; i < dgn.Length; i++)
            {
                if (dgn[i].Severity == DiagnosticSeverity.Error)
                {
                    // changes the flag
                    hasError = true;
                    // log full diagnostic info?
                    if (logFullDiagnInfo)
                    {
                        string id = dgn[i].Id;
                        int line = dgn[i].Location.GetLineSpan().StartLinePosition.Line;
                        string err = dgn[i].GetMessage();
                        errorInfo += $"{errorPrompt} {id} @line {line+1}: {err}";
                        if(i < dgn.Length - 1) { errorInfo += "\n"; }
                    }
                }
            }
            return (hasError, errorInfo);
        }

        /// <summary>
        /// checks for errors in multiple files
        /// within a given folder
        /// </summary>
        /// <param name="folderPath"> folder that contains all files to check </param>
        /// <param name="host"> Roslyn host used for the script generation </param>
        public static void CheckErrors(string folderPath,
            RoslynHost? host = null)
        {
            if (!Directory.Exists(folderPath)) { return; }
            host ??= CreateHost();
            // gets all files within the folder
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                FileInfo codeFile = new(file);
                // checks file extension
                if (codeFile.Extension.Equals(".txt")
                    || codeFile.Extension.Equals(".cs"))
                {
                    // reads the text from the code file
                    var codeText = new StreamReader(codeFile.FullName).ReadToEnd();
                    // creates script
                    Script<object> script = MakeScript(codeText, host);
                    // compiles and checks for errors
                    (bool hasError, string errorInfo) = CheckError(scr: script, 
                        logFullDiagnInfo: true);
                    if (hasError)
                    {
                        Printer.Logging(mainInfo: $"[{codeFile.Name}] contains error(s): ");
                        Printer.Write(errorInfo);
                        Printer.Write($"============================");
                    }
                }
            }


        }

    }

}
