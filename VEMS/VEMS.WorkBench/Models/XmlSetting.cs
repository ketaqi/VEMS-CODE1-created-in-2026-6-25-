using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using VEMS.MathCore;

namespace VEMS.WorkBench
{

    /// <summary>
    /// XML-based user-setting 
    /// </summary>
    public class XmlSetting: ObservableObject //INotifyPropertyChanged
    {
        //#region property changed ...

        ///// <summary>
        ///// 
        ///// </summary>
        //public event PropertyChangedEventHandler? PropertyChanged;

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="propertyName"></param>
        //protected virtual void OnPropertyChanged(string propertyName)
        //=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //#endregion
        #region properties 

        /// <summary>
        /// full path of the XML file
        /// </summary>
        internal string? XmlFilenName { get; set; }

        /// <summary>
        /// XML file that stores the user configuration
        /// </summary>
        internal XmlDocument XmlDoc { get; set; } = new();

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="xmlFileName"> full path of the xml file </param>
        internal XmlSetting(string xmlFileName)
        {
            // exception handling
            if (!File.Exists(xmlFileName)) { return; }
            // set properties
            XmlFilenName = xmlFileName;
            XmlDoc.Load(xmlFileName);
        }

        #endregion
        #region methods

        /// <summary>
        /// save Xml file
        /// </summary>
        public void SaveXml(bool showLogging = false)
        {
            // null case 
            if(XmlFilenName == null) { return; }
            // saves file
            XmlDoc.Save(filename: XmlFilenName);
            // logging
            if (showLogging)
            { Printer.Logging($"{GetType().Name} saved to {XmlFilenName}"); }
        }

        #endregion
    }


    /// <summary>
    /// user-preference parameters
    /// </summary>
    public partial class UserPreference : XmlSetting
    {
        #region properties

        #region ----- working directories -----

        /// <summary>
        /// internally stored working directory
        /// </summary>
        [ObservableProperty]
        private string workingDirectory
            = DirectoryHelper.SampleDirectory;

        ///// <summary>
        ///// working folder directory
        ///// </summary>
        //public string WorkingDirectory 
        //{
        //    get => workingDirectory;
        //    set
        //    {
        //        workingDirectory = value;
        //        OnPropertyChanged(nameof(WorkingDirectory));
        //    } 
        //}

        #endregion
        #region ----- fonts -----

        /// <summary>
        /// font family of code editor
        /// </summary>
        [ObservableProperty]
        private FontFamily codeEditorFontFamily
            = new (WorkBench.Defaults.CodeEditorFontFamily);

        /// <summary>
        /// font size of code editor
        /// </summary>
        [ObservableProperty]
        private double codeEditorFontSize 
            = WorkBench.Defaults.CodeEditorFontSize;

        /// <summary>
        /// font family of console
        /// </summary>
        [ObservableProperty]
        private FontFamily consoleFontFamily
            = new (WorkBench.Defaults.ConsoleFontFamily);

        /// <summary>
        /// font size of console
        /// </summary>
        [ObservableProperty]
        private double consoleFontSize
            = WorkBench.Defaults.ConsoleFontSize;

        /// <summary>
        /// list of font families
        /// </summary>
        public static List<FontFamily> FontFamilyList
        {
            get
            {
                List<FontFamily> result = new();
                foreach (var f in Fonts.SystemFontFamilies)
                { result.Add(f); }
                return result;
            }
        }

        /// <summary>
        /// list of font sizes
        /// </summary>
        public static List<double> FontSizeList
        {
            get
            {
                List<double> result = new()
                {
                    8, 9, 10, 10.5, 11, 12, 14, 16, 18, 20, 24, 28,
                    32, 36, 40, 44, 48, 54, 60, 66, 72, 80, 88, 96
                };
                return result;
            }
        }

        #endregion
        #region ----- number format -----

        /// <summary>
        /// culture option name
        /// </summary>
        public CultureName Culture
        {
            get => MathSettings.Culture;
            set
            {
                MathSettings.Culture = value;
                OnPropertyChanged(nameof(Culture));
            }
        }

        /// <summary>
        /// culture info
        /// </summary>
        public CultureInfo CultureInfo
        {
            get
            {
                string? cs = Culture.ToString();
                return cs != null ? new(cs) : CultureInfo.InvariantCulture;
            }
        }

        /// <summary>
        /// number format
        /// </summary>
        public NumericFormat NumberFormat
        {
            get => MathSettings.NumberFormat;
            set
            {
                MathSettings.NumberFormat = value;
                OnPropertyChanged(nameof(NumberFormat));
            }
        }

        /// <summary>
        /// number of digits
        /// </summary>
        public int NumberOfDigits
        {
            get => MathSettings.NumberOfDigits;
            set
            {
                MathSettings.NumberOfDigits = value;
                OnPropertyChanged(nameof(NumberOfDigits));
            }
        }

        /// <summary>
        /// representation format of complex number
        /// </summary>
        public ComplexFormat ComplexNumberFormat
        {
            get => MathSettings.ComplexNumberFormat;
            set
            {
                MathSettings.ComplexNumberFormat = value;
                OnPropertyChanged(nameof(ComplexNumberFormat));
            }
        }

        /// <summary>
        /// column separator
        /// </summary>
        public CharSeparator ColumnSeparator
        {
            get => MathSettings.ColumnSeparator;
            set
            {
                MathSettings.ColumnSeparator = value;
                OnPropertyChanged(nameof(ColumnSeparator));
            }
        }

        /// <summary>
        /// list of culture/region options
        /// </summary>
        public static List<CultureName> CultureList
        {
            get => new(collection:
                Enum.GetValues(typeof(CultureName)).Cast<CultureName>());
        }

        /// <summary>
        /// list of numeric formats
        /// </summary>
        public static List<NumericFormat> NumberFormatList
        {
            get => new(collection:
                Enum.GetValues(typeof(NumericFormat)).Cast<NumericFormat>());
        }

        /// <summary>
        /// list of complex number formats
        /// </summary>
        public static List<ComplexFormat> ComplexFormatList
        {
            get => new(collection:
                Enum.GetValues(typeof(ComplexFormat)).Cast<ComplexFormat>());
        }

        /// <summary>
        /// list of character separation format
        /// </summary>
        public static List<CharSeparator> CharSeparatorList
        {
            get => new(collection:
                Enum.GetValues(typeof(CharSeparator)).Cast<CharSeparator>());
        }

        #endregion

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="xmlFileName"> full path of the xml file </param>
        public UserPreference(string xmlFileName)
            : base(xmlFileName)
        {
            // initialization
            workingDirectory = DirectoryHelper.SampleDirectory;
        }

        #endregion
        #region methods

        /// <summary>
        /// loads user settings from XML file
        /// </summary>
        public void LoadSettingsFromXml()
        {
            XmlNode? n;
            // gets all parameters
            n = XmlDoc.SelectSingleNode("Preference/Directory/WorkingDirectory");
            if (n != null && File.Exists(n.InnerText)) { WorkingDirectory = n.InnerText; }
            n = XmlDoc.SelectSingleNode("Preference/Font/CodeEditor/FontFamily");
            if (n != null) { CodeEditorFontFamily = new(n.InnerText); };
            n = XmlDoc.SelectSingleNode("Preference/Font/CodeEditor/FontSize");
            if (n != null) { CodeEditorFontSize = double.Parse(n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Font/Console/FontFamily");
            if (n != null) { ConsoleFontFamily = new(n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Font/Console/FontSize");
            if (n != null) { ConsoleFontSize = double.Parse(n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Number/Culture");
            if (n != null) { Culture = (CultureName)Enum.Parse(enumType: typeof(CultureName), value: n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Number/NumberFormat");
            if (n != null) { NumberFormat = (NumericFormat)Enum.Parse(enumType: typeof(NumericFormat), value: n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Number/NumberOfDigits");
            if (n != null) { NumberOfDigits = int.Parse(n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Number/ComplexNumberFormat");
            if (n != null) { ComplexNumberFormat = (ComplexFormat)Enum.Parse(enumType: typeof(ComplexFormat), value: n.InnerText); }
            n = XmlDoc.SelectSingleNode("Preference/Number/ColumnSeparator");
            if (n != null) { ColumnSeparator = (CharSeparator)Enum.Parse(enumType: typeof(CharSeparator), value: n.InnerText); }
        }

        /// <summary>
        /// loads default directory settings
        /// </summary>
        [RelayCommand]
        public void LoadDefaultDirectorySettings()
        {
            WorkingDirectory = DirectoryHelper.SampleDirectory;
        }

        /// <summary>
        /// loads default font settings
        /// </summary>
        [RelayCommand]
        public void LoadDefaultFontSettings()
        {
            CodeEditorFontFamily = new(WorkBench.Defaults.CodeEditorFontFamily);
            CodeEditorFontSize = WorkBench.Defaults.CodeEditorFontSize;
            ConsoleFontFamily = new(WorkBench.Defaults.ConsoleFontFamily);
            ConsoleFontSize = WorkBench.Defaults.ConsoleFontSize;
        }

        /// <summary>
        /// loads default number settings
        /// </summary>
        [RelayCommand]
        public void LoadDefaultNumberSettings()
        {
            Culture = MathCore.Defaults.Culture;
            NumberFormat = MathCore.Defaults.NumberFormat; 
            NumberOfDigits = MathCore.Defaults.NumberOfDigits; 
            ComplexNumberFormat = MathCore.Defaults.ComplexNumberFormat; 
            ColumnSeparator = MathCore.Defaults.ColumnSeparator;
        }

        /// <summary>
        /// checks the working directory
        /// </summary>
        [RelayCommand]
        public void CheckWorkingDirectory()
        {
            Printer.Logging($"[Directory] Working Directory: {WorkingDirectory}");
        }

        /// <summary>
        /// checks the number of digits in MathSettings
        /// </summary>
        [RelayCommand]
        public void CheckNumberOfDigits()
        {
            Printer.Logging($"[MathSettings] Number of Digits: {MathSettings.NumberOfDigits}");
        }

        /// <summary>
        /// saves user settings to XML file
        /// </summary>
        [RelayCommand]
        public void SaveSettingsToXml()
        {
            XmlNode? n;
            // sets all parameters
            n = XmlDoc.SelectSingleNode("Preference/Directory/WorkingDirectory");
            if (n != null) { n.InnerText = WorkingDirectory; }
            n = XmlDoc.SelectSingleNode("Preference/Font/CodeEditor/FontFamily");
            if (n != null) { n.InnerText = CodeEditorFontFamily.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Font/CodeEditor/FontSize");
            if (n != null) { n.InnerText = CodeEditorFontSize.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Font/Console/FontFamily");
            if (n != null) { n.InnerText = ConsoleFontFamily.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Font/Console/FontSize");
            if (n != null) { n.InnerText = ConsoleFontSize.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Number/Culture");
            if (n != null) { n.InnerText = Culture.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Number/NumberFormat");
            if (n != null) { n.InnerText = NumberFormat.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Number/NumberOfDigits");
            if (n != null) { n.InnerText = NumberOfDigits.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Number/ComplexNumberFormat");
            if (n != null) { n.InnerText = ComplexNumberFormat.ToString(); }
            n = XmlDoc.SelectSingleNode("Preference/Number/ColumnSeparator");
            if (n != null) { n.InnerText = ColumnSeparator.ToString(); }

            // saves the XML file
            SaveXml(showLogging: true);
            //if (XmlFilenName != null)
            //{ 
            //    XmlDoc.Save(filename: XmlFilenName);
            //    Printer.Logging($"Preference settings saved to {XmlFilenName}");
            //}
        }

        #endregion
    }

    /// <summary>
    /// performance setting parameters
    /// </summary>
    public partial class PerformanceSetting : XmlSetting
    {
        #region properties

        #region ----- CPU -----

        /// <summary>
        /// number of physical cores of CPU
        /// </summary>
        public int NumberOfCPUCores
        {
            get => MathSettings.NumberOfCores;
            set
            {
                MathSettings.NumberOfCores = value;
                OnPropertyChanged(nameof(NumberOfCPUCores));
            }
        }

        #endregion
        #region ----- GPU -----

        // ...

        #endregion

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="xmlFileName"> full path of the xml file </param>
        public PerformanceSetting(string xmlFileName)
            : base(xmlFileName)
        { }

        #endregion
        #region methods

        /// <summary>
        /// loads user settings from XML file
        /// </summary>
        public void LoadSettingsFromXml()
        {
            XmlNode? n;
            // gets all parameters
            n = XmlDoc.SelectSingleNode("Performance/CPU/NumberOfCores");
            if (n != null) { NumberOfCPUCores = int.Parse(n.InnerText); }
        }

        /// <summary>
        /// loads default parameters
        /// </summary>
        [RelayCommand]
        public void LoadDefaultSettings()
        {
            // sets all parameters to default values
            NumberOfCPUCores = IntelMKL.GetMaxThreads();
        }

        /// <summary>
        /// checks the number of CPU cores in MathSettings
        /// </summary>
        [RelayCommand]
        public void CheckNumberOfCPUCores()
        {
            Printer.Logging($"[MathSettings] Number of CPU Cores: {MathSettings.NumberOfCores}");
        }

        /// <summary>
        /// saves user settings to XML file
        /// </summary>
        [RelayCommand]
        public void SaveSettingsToXml()
        {
            XmlNode? n;
            // sets all parameters
            n = XmlDoc.SelectSingleNode("Performance/CPU/NumberOfCores");
            if (n != null) { n.InnerText = NumberOfCPUCores.ToString(); }

            // saves the XML file
            SaveXml(showLogging: true);
            //if (XmlFilenName != null)
            //{
            //    Printer.Logging($"Performance settings saved to {XmlFilenName}");
            //    XmlDoc.Save(filename: XmlFilenName); 
            //}
        }

        #endregion
    }


    /// <summary>
    /// settings for math libraries
    /// </summary>
    [Obsolete]
    public class LibrarySetting : INotifyPropertyChanged
    {
        #region PropertyChanged ...

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


        private IBLAS _libraryValue { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IBLAS LibraryValue
        {
            get => _libraryValue;
            set
            {
                _libraryValue = value;
                OnPropertyChanged(nameof(Name));
                //PropertyChanged?.Invoke(this,
                //    new PropertyChangedEventArgs(nameof(LibraryValue)));
            }
        }

        //public void AutoTest() { }
    }


}
