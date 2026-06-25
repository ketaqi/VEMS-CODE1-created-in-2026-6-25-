//namespace VEMS.MathCore
//{

//    /// <summary>
//    /// user-setting parameters
//    /// </summary>
//    public static class UserSetting
//    {

//        #region ===== directories =====

//        // directory
//        private static string appDir = Directory.GetCurrentDirectory();
//        /// <summary>
//        /// gets the directory of the VEMS.WorkBench project
//        /// </summary>
//        internal static string ProjectDir
//        {
//            get
//            {
//                DirectoryInfo? di;
//                string d = new(appDir);
//                do
//                {
//                    di = Directory.GetParent(d);
//                    d = di == null ? string.Empty : di.ToString();
//                } while (Path.GetFileName(d) != "VEMS.WorkBench");
//                return d;
//            }
//        }

//        /// <summary>
//        /// gets the directory of the sample codes
//        /// </summary>
//        public static string SampleCodeDir => ProjectDir + @"\_sample";

//        /// <summary>
//        /// gets the xml file
//        /// </summary>
//        internal static string ConfigFolderFile => ProjectDir + @"\_config\ConfigPath.xml";
//        #endregion

//        #region properties

//        #region --- XmlHelper ---
//        private static XmlHelper xmlHelper;
//        #endregion

//        #region --- SelectPath ---

//        public static string FolderPath = SampleCodeDir;

//        #endregion

//        #region --- Preference ---

//        #region === Font ===

//        public static string CodeEditorFontFamily = "Cascadia Mono";
//        public static double CodeEditorFontSize = 18;
//        public static string OutputFontFamily = "Cascadia Mono";
//        public static double OutputFontSize = 16;

//        #endregion
//        #region === printer ===

//        /// <summary>
//        /// default number of digits
//        /// </summary>
//        public static int NumberOfDigits = 3;

//        /// <summary>
//        /// default number format
//        /// </summary>
//        public static NumericFormat NumberFormat = NumericFormat.Number;

//        /// <summary>
//        /// default representation format of complex number
//        /// </summary>
//        public static ComplexFormat ComplexNumberFormat = ComplexFormat.RealAndImaginary;

//        /// <summary>
//        /// default culture option name
//        /// </summary>
//        public static string CultureName = "cn-CN";

//        /// <summary>
//        /// column separator
//        /// </summary>
//        public static char ColumnSeparator = ',';

//        #endregion

//        #endregion

//        #region --- Performance ---

//        #region === CPU ===
//        /// <summary>
//        /// number of physical cores
//        /// </summary>
//        public static int NumberOfCore = 3;

//        public static int MaxCores = IntelMKLConfig.GetMaxThreads();

//        #endregion

//        #endregion

//        #endregion

//        #region methon

//        public static void LoadConfig()
//        {
//            if (!FileHelper.CheckIfFileExists(ConfigFolderFile)) { return; }

//            xmlHelper = new XmlHelper(ConfigFolderFile);

//            //根据标签获取节点对象(文件保存路径)
//            string selectPath = xmlHelper.GetNodeInnerText("//SelectPath");
//            if (!string.IsNullOrEmpty(selectPath))
//            { FolderPath = Directory.Exists(selectPath) ? selectPath : SampleCodeDir; }

//            //根据标签获取节点对象(CodeEditor字体)
//            string codeFontFamily = xmlHelper.GetNodeInnerText("//CodeEditor/FontFamily");
//            if (!string.IsNullOrEmpty(codeFontFamily))
//            { CodeEditorFontFamily = new(codeFontFamily); }
//            //根据标签获取节点对象(CodeEditor字体大小)
//            string codeFontFontSize = xmlHelper.GetNodeInnerText("//CodeEditor/FontSize");
//            if (!string.IsNullOrEmpty(codeFontFontSize))
//            { CodeEditorFontSize = double.Parse(codeFontFontSize); }

//            //根据标签获取节点对象(OutWindow字体)
//            string outWindowFontFamily = xmlHelper.GetNodeInnerText("//OutWindow/FontFamily");
//            if (!string.IsNullOrEmpty(outWindowFontFamily))
//            { OutputFontFamily = new(outWindowFontFamily); }
//            //根据标签获取节点对象(OutWindow字体大小)
//            string outWindowFontSize = xmlHelper.GetNodeInnerText("//OutWindow/FontSize");
//            if (!string.IsNullOrEmpty(outWindowFontSize))
//            { OutputFontSize = double.Parse(outWindowFontSize); }

//            //根据标签获取节点对象(默认有效位数)
//            string defaultDigits = xmlHelper.GetNodeInnerText("//Digits");
//            if (!string.IsNullOrEmpty(defaultDigits))
//            { NumberOfDigits = int.Parse(defaultDigits); }
//            //根据标签获取节点对象(默认实数格式)
//            string defaultNumericFormat = xmlHelper.GetNodeInnerText("//NumericFormat");
//            if (!string.IsNullOrEmpty(defaultNumericFormat))
//            { NumberFormat = (NumericFormat)Enum.Parse(typeof(NumericFormat), defaultNumericFormat); }
//            //根据标签获取节点对象(默认复数格式)
//            string defaultComplexFormat = xmlHelper.GetNodeInnerText("//ComplexFormat");
//            if (!string.IsNullOrEmpty(defaultComplexFormat))
//            { ComplexNumberFormat = (ComplexFormat)Enum.Parse(typeof(ComplexFormat), defaultComplexFormat); }

//            //根据标签获取节点对象(计算机核心数)
//            string NumCores = xmlHelper.GetNodeInnerText("//NumberOfCore");
//            if (!string.IsNullOrEmpty(NumCores))
//            { NumberOfCore = int.Parse(NumCores) > MaxCores ? MaxCores : int.Parse(NumCores); }
//        }


//        public static void SetConfig(string tag, string value)
//        {
//            switch (tag)
//            {
//                case "//SelectPath":
//                    FolderPath = value;
//                    break;
//                case "//CodeEditor/FontFamily":
//                    CodeEditorFontFamily = new(value);
//                    break;
//                case "//CodeEditor/FontSize":
//                    CodeEditorFontSize = int.Parse(value);
//                    break;
//                case "//OutWindow/FontFamily":
//                    OutputFontFamily = new(value);
//                    break;
//                case "//OutWindow/FontSize":
//                    OutputFontSize = int.Parse(value);
//                    break;
//                case "//Digits":
//                    NumberOfDigits = int.Parse(value);
//                    break;
//                case "//NumberOfCore":
//                    NumberOfCore = int.Parse(value);
//                    break;
//                case "//NumericFormat":
//                    NumberFormat = (NumericFormat)Enum.Parse(typeof(NumericFormat), value);
//                    break;
//                case "//ComplexFormat":
//                    ComplexNumberFormat = (ComplexFormat)Enum.Parse(typeof(ComplexFormat), value);
//                    break;
//                default:
//                    break;
//            }
//            //修改xml内对应的数据
//            xmlHelper.SetNodeInnerText(tag, value);
//            //保存配置文件
//            xmlHelper.SaveXml();
//        }
//        #endregion


//    }
//}
