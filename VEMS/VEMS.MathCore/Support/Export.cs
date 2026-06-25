using System.Text;

namespace VEMS.MathCore
{

    /// <summary>
    /// data exporter class
    /// <para> step #1: constructs a DataExporter </para>
    /// <para> step #2: converts data to string (as StringBuilder) </para> 
    /// <para> step #3: exports string (StringBuilder) to text file </para>
    /// </summary>
    public class DataExporter : DataConverter
    {
        #region properties

        // ...

        #endregion
        #region constructors

        /// <summary>
        /// constructs a DataConverter
        /// </summary>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> numeric format used for conversion </param>
        /// <param name="digits"> number of digits </param>
        /// <param name="culture"> culture selected for conversion </param>
        public DataExporter(CharSeparator? columnSeparator = CharSeparator.Comma, 
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
            : base(columnSeparator, numFormat, digits, culture)
        { }

        #endregion
        #region methods

        /// <summary>
        /// writes the content to a text file
        /// </summary>
        /// <param name="content"> content to export </param>
        /// <param name="targetPath"></param>
        /// <param name="chunkSize"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static async void ExportToText(StringBuilder content, string targetPath,
            int chunkSize = Defaults.ImExportChunkSize)
        {
            // consistency check
            if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
            { throw new DirectoryNotFoundException(nameof(targetPath)); }

            // stream writer for writing to file
            using (var writer = new StreamWriter(targetPath))
            {
                for (int i = 0; i < content.Length; i += chunkSize)
                {
                    int length = Math.Min(chunkSize, content.Length - i);
                    await writer.WriteAsync(content.ToString(i, length));
                }
            }
        }

        /// <summary>
        /// writes the content to a text file
        /// </summary>
        /// <param name="content"> content to export </param>
        /// <param name="targetPath"> target path of text file </param>
        [Obsolete]
        public async void ExportToText(StringBuilder content, string targetPath)
        {
            // consistency check
            if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
            { throw new DirectoryNotFoundException(nameof(targetPath)); }

            // write string to file
            // this method is relatively slow
            //File.WriteAllText(path: targetPath, contents: Content.ToString()); 
            using (var writer = new StreamWriter(targetPath))
            {
                // this method is faster
                await writer.WriteAsync(content.ToString());
                writer.Close();
                //// this method is very fast
                //int chunkSize = 4096;
                //for (int i = 0; i < Content.Length; i += chunkSize)
                //{
                //    int length = Math.Min(chunkSize, Content.Length - i);
                //    await writer.WriteAsync(Content.ToString(i, length));
                //} 
            }
        }
    
        #endregion
    }

}
