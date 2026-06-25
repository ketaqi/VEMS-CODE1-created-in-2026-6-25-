
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using Avalonia.Media;
using Avalonia.Platform;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;

using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using System.Text;
using RoslynPad.FeatureDemos.Views;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Models
{
    [AutoCollapseCategories("AutoCollapse")]
    public class SimpleObject1 : ReactiveObject
    {
#pragma warning disable CA1051 // 不要声明可见实例字段
        public readonly string Description;
#pragma warning restore CA1051 // 不要声明可见实例字段
        //public IImage? AvaloniaBanner { get; set; }


#pragma warning disable CA1051 // 不要声明可见实例字段
        public readonly MainViewModel _mainViewModel;
#pragma warning restore CA1051 // 不要声明可见实例字段
        public SimpleObject1(MainViewModel mainViewModel, string description)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            Description = description ;

            var families = _mainViewModel.SystemFontFamilies;
            var names = families
                .Select(f => f.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToArray();

            var defaultName = _mainViewModel.EditorFontFamily1
                              ?? _mainViewModel.FftLibraryInfo
                              ?? names.FirstOrDefault()
                              ?? string.Empty;
            Shortcut = new LoginInfo1(mainViewModel);

        }

        [Category("Path")]
        [DisplayName("leftMenuItem221")]
        [PathBrowsable(PathBrowsableType.Directory, InitialFileName = "C:\\Users")]
        [Watermark("Select Home")]
        [Description("Select the user's home directory")]
        public string UserHome
        {
            get
            {
                return _mainViewModel.InitialFolderPath;
            }

            set
            {
                if (_mainViewModel.InitialFolderPath != value)
                {
                    _mainViewModel.InitialFolderPath = value;

                }
                Debug.WriteLine("11111");
                Debug.WriteLine(value);
            }
        }

        [Category("leftMenuItem23")]
        [DisplayName("leftMenuItem231")]
        [Description("可选字体列表 (下拉)")]
        [SingleSelectionMode(SingleSelectionMode.Default)]
        public FontFamily FontFamilyNames1
        {
            get
            {
                
                return _mainViewModel.SelectedFontFamilyObject ?? new FontFamily("Courier New");
            }
            set
            {
                if (_mainViewModel.SelectedFontFamilyObject != value)
                {
                    _mainViewModel.SelectedFontFamilyObject = value.Name;

                }
            }
        }

        [Category("leftMenuItem23")]
        [DisplayName("leftMenuItem232")]
        [Description("可选字体列表 (下拉)")]
        [SingleSelectionMode(SingleSelectionMode.Default)]
        public FontFamily FontFamilyNames2
        {
            get
            {
                return _mainViewModel.SelectedFontFamilyObject1 ?? new FontFamily("Courier New");
            }
            set
            {
                if (_mainViewModel.SelectedFontFamilyObject1 != value)
                {
                    _mainViewModel.SelectedFontFamilyObject1 = value.Name;

                }
            }
        }

        //[Category("Font")]
        //[DisplayName("Editor Font Sizes")]
        //[Description("从 MainViewModel 获取的编辑器可选字号列表，用于展示和选择。")]
        //[SingleSelectionMode(SingleSelectionMode.Default)]
        private SelectableList<int> _editorFontSizes = new SelectableList<int>(Array.Empty<int>());
        [Category("leftMenuItem23")]
        [DisplayName("leftMenuItem233")]
        [Description("当前选中的编辑器字号，与上面的下拉列表联动")]
        public int SelectedEditorFontSize
        {
            get => _mainViewModel.EditorFontSize;
            set
            {
                if (_mainViewModel.EditorFontSize != value)
                {
                    // 回写到 VM（等价于 SelectedItem=TwoWay）
                    _mainViewModel.EditorFontSize = value;

                    

                    RaisePropertyChanged(nameof(SelectedEditorFontSize));
                }
            }
        }

        //[Category("Font")]
        //[DisplayName("Output Font Size (List)")]
        //[Description("输出区字号下拉，选择后回写到 MainViewModel.OutputFontSize")]
        //[SingleSelectionMode(SingleSelectionMode.Default)]
        private SelectableList<int> _outputFontSizeList = new SelectableList<int>(Array.Empty<int>());

        [Category("leftMenuItem23")]
        [DisplayName("leftMenuItem234")]
        [Description("当前选中的输出区字号，与 OutputFontSizeList 联动")]
        public int SelectedOutputFontSize
        {
            get => _mainViewModel.OutputFontSize;
            set
            {
                if (_mainViewModel.OutputFontSize != value)
                {
                    // 回写到 VM
                    _mainViewModel.OutputFontSize = value;

                    

                    RaisePropertyChanged(nameof(SelectedOutputFontSize));
                }
            }
        }

        [Category("leftMenuItem24")]
        [DisplayName("leftMenuItem241")]
        [Description("待完成的数字格式区域")]
        public SelectableList<int> OutputFontSizeList5
        {
            get => _outputFontSizeList;
            set
            {
                if (!ReferenceEquals(_outputFontSizeList, value))
                {

                    _outputFontSizeList = value ?? new SelectableList<int>(Array.Empty<int>());

                    RaisePropertyChanged(nameof(OutputFontSizeList5));
                }
            }
        }

        //下边是快捷键相关设置

        [Category("Shortcut")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("Shortcut")]
        [Description("快捷键区域")]
        
        public LoginInfo1? Shortcut { get; set; }


    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CategoryOrderExampleObject
    {
        [Category("Name")] public string? Property0 { get; set; } = "First";

        [Category("Name")] public string? Property1 { get; set; } = "Second";

        [Category("Name2")] public string? Property2 { get; set; } = "Third";

        [Category("Name2")] public string? Property3 { get; set; } = "Fourth";
    }


}
