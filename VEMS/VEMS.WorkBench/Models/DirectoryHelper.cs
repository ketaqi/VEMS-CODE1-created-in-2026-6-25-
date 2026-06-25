using System.IO;

namespace VEMS.WorkBench
{
    /// <summary>
    /// helper functions for directories
    /// </summary>
    public class DirectoryHelper
    {
        /// <summary>
        /// application directory
        /// </summary>
        internal static string AppDirectory 
            => Directory.GetCurrentDirectory();

        /// <summary>
        /// VEMS.WorkBench project directory 
        /// </summary>
        internal static string ProjectDirectory
        {
            get
            {
                DirectoryInfo? di;
                string d = new(AppDirectory);
                do
                {
                    di = Directory.GetParent(d);
                    d = di == null ? string.Empty : di.ToString();
                } while (Path.GetFileName(d) != "VEMS.WorkBench");
                return d;
            }
        }

        /// <summary>
        /// sample directory
        /// </summary>
        public static string SampleDirectory 
            => ProjectDirectory + @"\_sample";

        /// <summary>
        /// configuration directory
        /// </summary>
        public static string ConfigDirectory 
            => ProjectDirectory + @"\_config";
    }
}
