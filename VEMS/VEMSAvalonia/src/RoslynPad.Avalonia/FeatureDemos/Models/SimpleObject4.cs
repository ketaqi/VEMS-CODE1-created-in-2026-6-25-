
using System.Collections.ObjectModel;
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
using NuGet.Protocol.Plugins;
using Newtonsoft.Json.Linq;
using RoslynPad.FeatureDemos.Views;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Models
{
    [AutoCollapseCategories("AutoCollapse")]
    public class SimpleObject4 : ReactiveObject
    {
#pragma warning disable CA1051 // 不要声明可见实例字段
        public readonly string Description;
#pragma warning restore CA1051 // 不要声明可见实例字段

        // 现在强制为非空
        private readonly MainViewModel _mainViewModel;

        //private readonly OpenDocumentViewModel? _currentOpenDocument;
        // public IImage? AvaloniaBanner { get; set; }
        // 仅保留必须传入 MainViewModel 的构造函数
        public SimpleObject4(MainViewModel mainViewModel, string description)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            Description = description;

            Console.WriteLine("SimpleObject40 running");
            var AvailablePlatformNames1 = _mainViewModel.AvailablePlatformNames ?? Array.Empty<string>();
            SelectableList<string> selectable = new SelectableList<string>(AvailablePlatformNames1);
            EditorFontSizes41 = new SelectableList<string>(selectable);
            Console.WriteLine("SimpleObject41 running");
            var ProcessFilterOptions = _mainViewModel.ProcessFilterOptions;
            SelectableList<string> selectable1 = new SelectableList<string>(ProcessFilterOptions);
            EditorFontSizes42 = new SelectableList<string>(selectable1);
            Console.WriteLine("SimpleObject42 running");
            //_mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
            EditorFontSizes41.SelectionChanged += EditorFontSizes_SelectionChanged;
        }

        private void EditorFontSizes_SelectionChanged(object? sender, EventArgs e)
        {
            // 优先使用触发事件的实例（sender），避免因属性引用被替换而读取到旧实例或 null
            var list = sender as PropertyModels.Collections.SelectableList<string> ?? EditorFontSizes41;

            // 仍做空检查并打印调试信息
            var selected = list?.SelectedValue;
            Console.WriteLine($"[DEBUG] SelectionChanged fired. SelectedValue = '{selected ?? "<null>"}'");
            Console.WriteLine("SelectedValue");

            if (selected is string sv && _mainViewModel.CurrentOpenDocument is { } cur)
            {
                // 幂等写入，避免循环触发
                //if (!string.Equals(cur.SelectedPlatformName, sv, StringComparison.Ordinal))
                //{
                    cur.SelectedPlatformName = sv; 
                    Console.WriteLine(sv);
                    Console.WriteLine(cur.SelectedPlatformName);
                    Console.WriteLine("SelectedValue running");
                //}
            }
        }

        [Category("leftMenuItem351")]
        [DisplayName("leftMenuItem352")]
        [PropertyButton("Run", MethodName = "Run")]
        // 关键：让该行显示操作列
        public int Run { get; set; } = 10;

        [Category("leftMenuItem351")]
        [DisplayName("leftMenuItem353")]
        [PropertyButton("Terminate", MethodName = "Terminate")]
        // 关键：让该行显示操作列
        public int Terminate { get; set; } = 10;

        [Category("leftMenuItem351")]
        [DisplayName("leftMenuItem356")]
        [PropertyButton("SetDefaultPlatform", MethodName = "SetDefaultPlatform")]
        // 关键：让该行显示操作列
        public int SetDefaultPlatform { get; set; } = 10;


        public void OnActionExecuted()
        {
            System.Diagnostics.Debug.WriteLine("Action executed on Quantity.");
        }



        [Category("leftMenuItem351")]
        [DisplayName("leftMenuItem355")]
        [Description("Please Select login name from list")]
        public SelectableList<string> EditorFontSizes41
        {
            get => _editorFontSizes41;
            set
            {
                // 如果是同一个实例，确保订阅存在并返回
                if (ReferenceEquals(_editorFontSizes41, value))
                {
                    // 保证已订阅（可能尚未订阅）
                    if (_editorFontSizes41 != null)
                    {
                        //_editorFontSizes41.SelectionChanged -= EditorFontSizes_SelectionChanged; // 安全退订（若未订阅会忽略）
                        //_editorFontSizes41.SelectionChanged += EditorFontSizes_SelectionChanged;
                    }
                    RaisePropertyChanged(nameof(EditorFontSizes41));
                    return;
                }

                //// 退订旧实例
                //if (_editorFontSizes41 != null)
                //{
                //    try { _editorFontSizes41.SelectionChanged -= EditorFontSizes_SelectionChanged; } catch { }
                //}

                // 赋新实例（防 null）
                _editorFontSizes41 = value ?? new SelectableList<string>(Array.Empty<string>());

                // 在新实例上订阅
                //_editorFontSizes41.SelectionChanged += EditorFontSizes_SelectionChanged;

                RaisePropertyChanged(nameof(EditorFontSizes41));
            }
        }
        
        private SelectableList<string> _editorFontSizes41 = new SelectableList<string>(Array.Empty<string>());

        [Category("leftMenuItem351")]
        [DisplayName("leftMenuItem354")]
        [Description("Please Select login name from list")]
        public SelectableList<string> EditorFontSizes42
        {
            get => _editorFontSizes42;
            set
            {
                if (!ReferenceEquals(_editorFontSizes42, value))
                {
                    _editorFontSizes42 = value ?? new SelectableList<string>(Array.Empty<string>());
                    RaisePropertyChanged(nameof(EditorFontSizes42));
                }
            }
        }

        private SelectableList<string> _editorFontSizes42 = new SelectableList<string>(Array.Empty<string>());
    
    }
   

    // 简单的 RelayCommand 实现

    [Flags]
    
    public enum PhoneService4
    {
        [EnumDisplayName("Default")]
        None = 0,
        LandLine = 1,
        Cell = 2,
        Fax = 4,
        Internet = 8,
        Other = 16,

        [EnumExclude]
        CanNotSeeThis = 32
    }


}
