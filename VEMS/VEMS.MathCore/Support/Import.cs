using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// Import class
    /// </summary>
    public class Import
    {
        #region ---- helper ----

        #region === Judge complex structure ===
        /// <summary>
        /// convert a+bi text data to complex format data
        /// </summary>
        /// <param name="num"> a complex number in string format </param>
        /// <returns> return a complex number </returns>
        private static Complex ConverterCplx(string num)
        {
            string number = num.Replace(" ", "").Replace("(", "").Replace(")", "");
            string[] parts = number.Replace("i", "").Split(new char[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
            double realPart;
            if (number.StartsWith("-"))
            { realPart = -double.Parse(parts[0]); }
            else
            { realPart = double.Parse(parts[0]); }
            double imaginaryPart;
            if (number.IndexOf('-', 1) != -1)
            { imaginaryPart = -double.Parse(parts[1]); }
            else
            { imaginaryPart = double.Parse(parts[1]); }

            Complex res = new(realPart, imaginaryPart);

            return res;
        }

        /// <summary>
        /// construct a complex number with two doubles
        /// </summary>
        /// <param name="num1"> the real part of the complex number. </param>
        /// <param name="num2"> the imaginary part of the complex number. </param>
        /// <returns> return a complex number </returns>
        private static Complex ConstructCplx(string num1, string num2)
        {
            Complex res = new(double.Parse(num1), double.Parse(num2));
            return res;
        }
        #endregion
        #region === File data ===
        /// <summary>
        /// read the file directly and get all the text data in the file
        /// </summary>
        /// <param name="filePath"> file path </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> returns an enumerable collection </returns>
        private static IEnumerable<string>? GetData(string filePath
            , int startLine = 0
            , int endLine = int.MaxValue
            , bool skipComment = true
            , bool filterBlankLines = false)
        {
            IEnumerable<string>? lines = null;
            if (startLine >= 0 && endLine > startLine )
            {
                if (endLine > int.MaxValue) { endLine = int.MaxValue; }
                lines = File.ReadLines(filePath).Skip(startLine).Take((endLine - startLine));
            }
            else 
            {
                Printer.Warning($"The end line must be larger than the start line.");
                return lines; 
            }

            if (filterBlankLines)
            {
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line));
            }
            
            //跳过开头井号注释行
            if(skipComment)
            {
                lines = lines.SkipWhile((line) => line.StartsWith('#'));
            }
            
            return lines;
        }
        #endregion

        #endregion
        #region ---- kernels ----

        #region === VectorD/Z ===
        private static VectorD? Text2VectorD(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try {
                List<double> vector = new();
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                foreach (var line in lines)
                {
                    var values = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    vector.AddRange(values.Select(numStr => double.Parse(numStr)));
                }
                VectorD v = new(vector.Count);
                for (int i = 0; i < vector.Count; i++)
                {
                    v[i] = vector[i];
                }
                return v;
            } 
            catch 
            {
                Printer.Warning($"File import failure.");
                return null; 
            }
        }

        private static VectorZ? Text2VectorZ(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try {
                List<Complex> vector = new();
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                foreach (var line in lines)
                {
                    var values = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    vector.AddRange(values.Select(numStr => ConverterCplx(numStr)));
                }

                VectorZ z = new(vector.Count);
                for (int i = 0; i < vector.Count; i++)
                {
                    z[i] = vector[i];
                }
                return z;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        #endregion
        #region === MatrixD/Z ===
        private static MatrixD? Text2MatrixD(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

                MatrixD mD = new(rows, cols);
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    for (long iCol = 0; iCol < cols; iCol++)
                    {
                        mD[iRow, iCol] = double.Parse(nums[iCol]);
                    }
                    iRow++;
                }
                return mD;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }

        //private static MatrixD? Text2MatrixDRev(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true)
        //{
        //    try {
        //        IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment);

        //        long rows = lines.Count();
        //        long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

        //        MatrixD mD = new(cols, rows);
        //        long iRow = 0;
        //        foreach (var line in lines)
        //        {
        //            string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
        //            for (long iCol = 0; iCol < cols; iCol++)
        //            {
        //                mD[iCol, iRow] = double.Parse(nums[iCol]);
        //            }
        //            iRow++;
        //        }
        //        return mD;
        //    }
        //    catch
        //    {
        //        Printer.Warning($"File import failure.");
        //        return null;
        //    }
        //}

        private static MatrixZ? Text2MatrixZ(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

                MatrixZ mZ = new(rows, cols);
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    for (long iCol = 0; iCol < cols; iCol++)
                    {
                        mZ[iRow, iCol] = ConverterCplx(nums[iCol]);
                    }
                    iRow++;
                }
                return mZ;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }

        //private static MatrixZ? Text2MatrixZRev(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true)
        //{
        //    try
        //    {
        //        IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment);
        //        long rows = lines.Count();
        //        long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

        //        MatrixZ mZ = new(cols, rows);
        //        long iRow = 0;
        //        foreach (var line in lines)
        //        {
        //            string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
        //            for (long iCol = 0; iCol < cols; iCol++)
        //            {
        //                mZ[iCol, iRow] = ConverterCplx(nums[iCol]);
        //            }
        //            iRow++;
        //        }
        //        return mZ;
        //    }
        //    catch
        //    {
        //        Printer.Warning($"File import failure.");
        //        return null;
        //    }
        //}

        #endregion

        #region === Scat1D[Real/Cplx]Data ===
        private static Scat1DRealData? Text2Scat1DRealData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();
                VectorD poi = new(rows);
                VectorD vec = new(rows);
                
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    for (long iCol = 0; iCol < cols; iCol++)
                    {
                        poi[iRow] = double.Parse(nums[0]);
                        vec[iRow] = double.Parse(nums[1]);
                    }
                    iRow++;
                }
                Scat1DRealData sdrData = new(poi, vec);
                return sdrData;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        

        public static Scat1DCplxData? Text2Scat1DCplxData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            { // 读取文件的所有行
                var lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines).ToList();
                int startIndex = 0;
                int endIndex = lines.Count;
                if (startLine == 0) 
                { 
                    // 找到数据开始的行索引
                    startIndex = lines.FindIndex(line => line.Contains("data: |") || line.Contains("Wavelength (nm)")) == -1 ? startLine : lines.FindIndex(line => line.Contains("data: |") || line.Contains("Wavelength (nm)"))+1;  
                }
                endIndex = lines.FindIndex(line => line.Contains("SPECS:")) == -1 ? endIndex : lines.FindIndex(line => line.Contains("SPECS:"));
                // 获取数据行
                //var dataLines = lines.Skip(startIndex + 1).TakeWhile(line => !line.Contains("SPECS:"));
                List<string> subLines = lines.GetRange(startIndex, endIndex - startIndex);

                // 计算数据行数
                int dataCount = subLines.Count();

                //创建基础数据类
                VectorD poi = new(dataCount);
                VectorZ vec = new(dataCount);
                int iRow = 0;

                // 存储数据行
                foreach (var line in subLines)
                {
                    var numbers = line.Split(columnSeparator).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    if (numbers.Length == 2)
                    {
                        poi[iRow] = double.Parse(numbers[0]);
                        vec[iRow] = ConverterCplx(numbers[1]);
                        iRow++;
                    }
                    else
                    {
                        poi[iRow] = double.Parse(numbers[0]);
                        vec[iRow] = ConstructCplx(numbers[1], numbers[2]);
                        iRow++;
                    }
                }
                Scat1DCplxData sdcData = new(poi, vec);
                return sdcData;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        #endregion
        #region === Grid1D[Real/Cplx]Data ===
        private static Grid1DRealData? Text2Grid1DRealData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

                double GridInfoInitval = 0;
                double GridInfoSecond = 1;
                double GridInfoSpacing = 1;
                List<string> firstTwoLines = lines.Take(2).ToList();
                GridInfoInitval =double.Parse(firstTwoLines[0].Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries)[0]);
                if (rows>1)
                {
                    GridInfoSecond = double.Parse(firstTwoLines[1].Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries)[0]);
                    GridInfoSpacing = GridInfoSecond - GridInfoInitval;
                }
                GridInfo1D grid = new(rows, GridInfoInitval, GridInfoSpacing);

                VectorD values = new(rows);
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] numbers = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    values[iRow] = double.Parse(numbers[1]);
                    iRow++;
                }
                
                Grid1DRealData gdrData = new(values: values, gridInfo: grid);
                return gdrData;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        private static Grid1DRealData? Text2Grid1DRealData(string filePath, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            VectorD values = Text2VectorD(filePath,columnSeparator,startLine, endLine, skipComment, filterBlankLines);
            GridInfo1D grid = new(values.Count, gridSpacing);
            Grid1DRealData gdrData = new(values: values, gridInfo: grid);
            return gdrData;
        }
        private static Grid1DRealData? Text2Grid1DRealData(string filePath, double initVal, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            VectorD values = Text2VectorD(filePath, columnSeparator , startLine , endLine, skipComment, filterBlankLines);
            GridInfo1D grid = new(values.Count, initVal, gridSpacing);
            Grid1DRealData gdrData = new(values: values, gridInfo: grid);
            return gdrData;
        }

        private static Grid1DCplxData? Text2Grid1DCplxData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

                double GridInfoInitval = 0;
                double GridInfoSecond = 1;
                double GridInfoSpacing = 1;
                List<string> firstTwoLines = lines.Take(2).ToList();
                GridInfoInitval = double.Parse(firstTwoLines[0].Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries)[0]);
                if (rows > 1)
                {
                    GridInfoSecond = double.Parse(firstTwoLines[1].Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries)[0]);
                    GridInfoSpacing = GridInfoSecond - GridInfoInitval;
                }
                GridInfo1D grid = new(rows, GridInfoInitval, GridInfoSpacing);

                VectorZ values = new(rows);
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] numbers = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    values[iRow] = values[iRow] = ConverterCplx(numbers[1]); ;
                    iRow++;
                }

                Grid1DCplxData gdcData = new(values: values, gridInfo: grid);
                return gdcData;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        private static Grid1DCplxData? Text2Grid1DCplxData(string filePath, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            VectorZ values = Text2VectorZ(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines);
            GridInfo1D grid = new(values.Count, gridSpacing);
            Grid1DCplxData gdcData = new(values: values, gridInfo: grid);
            return gdcData;
        }
        private static Grid1DCplxData? Text2Grid1DCplxData(string filePath, double initVal, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            VectorZ values = Text2VectorZ(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines);
            GridInfo1D grid = new(values.Count,initVal, gridSpacing);
            Grid1DCplxData gdcData = new(values: values, gridInfo: grid);
            return gdcData;
        }
        #endregion

        #region === Scat2D[Real/Cplx]Data ===
        private static Scat2DRealData? Text2Scat2DRealData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

                VectorD pX = new(rows);
                VectorD pY = new(rows);
                VectorD val = new(rows);
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    pX[iRow] = double.Parse(nums[0]);
                    pY[iRow] = double.Parse(nums[1]);
                    val[iRow] = double.Parse(nums[2]);
                    iRow++;
                }
                Scat2DRealData s2drData = new(scatInfo: new(pY, pX), values: val);
                return s2drData;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        private static Scat2DCplxData? Text2Scat2DCplxData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            try
            {
                IEnumerable<string> lines = GetData(filePath, startLine, endLine, skipComment, filterBlankLines);
                long rows = lines.Count();
                long cols = lines.FirstOrDefault().Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries).Count();

                VectorD pX = new(rows);
                VectorD pY = new(rows);
                VectorZ val = new(rows);
                long iRow = 0;
                foreach (var line in lines)
                {
                    string[] nums = line.Trim().Split(columnSeparator, StringSplitOptions.RemoveEmptyEntries);
                    pX[iRow] = double.Parse(nums[0]);
                    pY[iRow] = double.Parse(nums[1]);
                    val[iRow] = ConverterCplx(nums[2]);
                    iRow++;
                }
                Scat2DCplxData s2dcData = new(scatInfo: new(pY, pX), values: val);
                return s2dcData;
            }
            catch
            {
                Printer.Warning($"File import failure.");
                return null;
            }
        }
        #endregion
        #region === Grid2D[Real/Cplx]Data ===
        //Have no idea
        #endregion

        #region === ScatXY[Real/Cplx]Data ===
        //Have no idea
        #endregion

        #endregion
        #region ---- wrappers ----
        #region === VectorD/Z ===
        /// <summary>
        /// reads data from a text file to a vector 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns>  return VectorD </returns>
        public static VectorD? Txt2VectorD(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)){ return null; }
            return Text2VectorD(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines);
        }

        /// <summary>
        /// reads data from a text file to a vector 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return  VectorZ </returns>
        public static VectorZ? Txt2VectorZ(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2VectorZ(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines);
        }

        #endregion
        #region === MatrixD/Z ===
        /// <summary>
        /// reads data from a text file to real-valued 2D data
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return MatrixD </returns>
        public static MatrixD? Txt2MatrixD(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2MatrixD(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines);
        }
        ///// <summary>
        ///// reads data from a text file to real-valued 2D data with reversed row sequence
        ///// </summary>
        ///// <param name="filePath"> source file containing the data </param>
        ///// <param name="columnSeparator"> character that separates the columns </param>
        ///// <param name="startLine"> start storing rows of data </param>
        ///// <param name="endLine"> end storing rows of data </param>
        ///// <param name="skipComment"> whether to skip comment lines </param>
        ///// <returns> return reverse MatrixD </returns>
        //public static MatrixD? Txt2MatrixDRev(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true)
        //{
        //    // check if the file path exists
        //    if (!FileHelper.CheckIfFileExists(filePath)) { return null; }
        //    return Text2MatrixDRev(filePath, columnSeparator, startLine, endLine, skipComment);
        //}

        /// <summary>
        /// reads data from a text file to complex-valued 2D data 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return MatrixZ </returns>
        public static MatrixZ? Txt2MatrixZ(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2MatrixZ(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines);
        }
        ///// <summary>
        ///// reads data from a text file to complex-valued 2D data with reversed row sequence 
        ///// </summary>
        ///// <param name="filePath"> sSource file containing the data </param>
        ///// <param name="columnSeparator"> character that separates the columns </param>
        ///// <param name="startLine"> start storing rows of data </param>
        ///// <param name="endLine"> end storing rows of data </param>
        ///// <param name="skipComment"> whether to skip comment lines </param>
        ///// <returns> return reverse MatrixZ </returns>
        //public static MatrixZ? Txt2MatrixZRev(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true)
        //{
        //    // check if the file path exists
        //    if (!FileHelper.CheckIfFileExists(filePath)) { return null; }
        //    return Text2MatrixZRev(filePath, columnSeparator, startLine, endLine, skipComment);
        //}
        #endregion

        #region === Scat1D[Real/Cplx]Data ===
        /// <summary>
        /// reads data from a text file to real-valued 1D scattered data 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Scat1DRealData </returns>
        public static Scat1DRealData? Txt2Scat1DRealData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Scat1DRealData(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }

        /// <summary>
        /// reads data from a text file to complex-valued 1D scattered data
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Scat1DCplxData </returns>
        public static Scat1DCplxData? Txt2Scat1DCplxData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Scat1DCplxData(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }


        #endregion
        #region === Grid1D[Real/Cplx]Data ===
        /// <summary>
        /// reads data from a text file to real-valued 1D grid data 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Grid1DRealData </returns>
        public static Grid1DRealData? Txt2Grid1DRealData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Grid1DRealData(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }
        /// <summary>
        /// reads data from a text file to real-valued 1D grid data 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="gridSpacing"> grid spacing </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Grid1DRealData </returns>
        public static Grid1DRealData? Txt2Grid1DRealData(string filePath, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Grid1DRealData(filePath, gridSpacing, columnSeparator,  startLine, endLine, skipComment, filterBlankLines); 
        }
        /// <summary>
        /// reads data from a text file to real-valued 1D grid data
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="initVal"> first grid point </param>
        /// <param name="gridSpacing"> grid spacing </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Grid1DRealData </returns>
        public static Grid1DRealData? Txt2Grid1DRealData(string filePath, double initVal, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Grid1DRealData(filePath, initVal, gridSpacing, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }

        /// <summary>
        /// reads data from a text file to complex-valued 1D grid data 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Grid1DCplxData </returns>
        public static Grid1DCplxData? Txt2Grid1DCplxData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Grid1DCplxData(filePath, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }
        /// <summary>
        /// reads data from a text file to complex-valued 1D grid data 
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="gridSpacing"> grid spacing </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Grid1DCplxData </returns>
        public static Grid1DCplxData? Txt2Grid1DCplxData(string filePath, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Grid1DCplxData(filePath, gridSpacing, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }
        /// <summary>
        /// reads data from a text file to complex-valued 1D grid data
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="initVal"> girst grid point </param>
        /// <param name="gridSpacing"> grid spacing </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Grid1DCplxData </returns>
        public static Grid1DCplxData? Txt2Grid1DCplxData(string filePath, double initVal, double gridSpacing, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Grid1DCplxData(filePath, initVal, gridSpacing, columnSeparator, startLine, endLine, skipComment, filterBlankLines); 
        }
        #endregion

        #region === Scat2D[RealCplx]Data ===
        /// <summary>
        /// reads data from a text file to real-valued 2D scattered data
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Scat2DRealData </returns>
        public static Scat2DRealData? Txt2Scat2DRealData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Scat2DRealData(filePath, columnSeparator, startLine,endLine,skipComment, filterBlankLines); 
        }
        /// <summary>
        /// reads data from a text file to complex-valued 2D scattered data to text
        /// </summary>
        /// <param name="filePath"> source file containing the data </param>
        /// <param name="columnSeparator"> character that separates the columns </param>
        /// <param name="startLine"> start storing rows of data </param>
        /// <param name="endLine"> end storing rows of data </param>
        /// <param name="skipComment"> whether to skip comment lines(default true) </param>
        /// <param name="filterBlankLines"> Whether to filter blank lines(default fasle) </param>
        /// <returns> return Scat2DRealData </returns>
        public static Scat2DCplxData? Txt2Scat2DCplxData(string filePath, char columnSeparator = ',', int startLine = 0, int endLine = int.MaxValue, bool skipComment = true, bool filterBlankLines = false)
        {
            // check if the file path exists
            if (!File.Exists(filePath)) { return null; }
            return Text2Scat2DCplxData(filePath, columnSeparator, startLine,endLine, skipComment, filterBlankLines); 
        }
        #endregion
        #region === Grid2D[Real/Cplx]Data ===
        //Have no idea
        #endregion

        #region === ScatXY[Real/Cplx]Data ===
        //Have no idea
        #endregion

        #endregion
    }
}
