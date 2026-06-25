//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace VEMS.MathCore
//{
//    internal class FileHelper
//    {

//        /// <summary>
//        /// checks if the target file path exists
//        /// if not, print warning informatino
//        /// </summary>
//        /// <param name="filePath"> target file path (full) </param>
//        /// <returns> whether the path exists </returns>
//        internal static bool CheckIfPathExists(string filePath)
//        {
//            bool flg = Directory.Exists(Path.GetDirectoryName(filePath));
//            if (!flg) { Printer.Warning($"The path '{filePath}' does not exist."); }
//            return flg;
//        }

//        /// <summary>
//        /// checks if the target file exists
//        /// if not, print warning informatino
//        /// </summary>
//        /// <param name="filePath"> target file path (full) </param>
//        /// <returns> whether the file exists </returns>
//        internal static bool CheckIfFileExists(string filePath)
//        {
//            bool flg = File.Exists(filePath);
//            if (!flg) { Printer.Warning($"The file in this '{filePath}' does not exist."); }
//            return flg;
//        }

//    }
//}
