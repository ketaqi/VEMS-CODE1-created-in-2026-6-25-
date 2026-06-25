
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using NuGet.Protocol.Plugins;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using RoslynPad.Editor;
using RoslynPad.FeatureDemos.Views;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Models
{
    [AutoCollapseCategories("AutoCollapse")]
    public class SimpleObject3 : ReactiveObject
    {

#pragma warning disable CA1051 // 不要声明可见实例字段
        public readonly string Description;
#pragma warning restore CA1051 // 不要声明可见实例字段

        // 现在强制为非空
        private readonly MainViewModel _mainViewModel;
        private NuGetDocumentViewModel? _nuget;
        public SimpleObject3(MainViewModel mainViewModel, string description)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            
            Description = description;

            // 安全取 Packages（避免 null）
            var packages = _mainViewModel.CurrentOpenDocument?.NuGet?.Packages ?? Array.Empty<PackageData>();

            // 将 PackageData 投影成 string，比如用 Id（也可以用 $"{p.Id} {p.Version}"）
            var names = packages.Select(p => p.Id);

            // 创建 SelectableList<string>
            var selectable = new SelectableList<string>(names);

            // 直接赋值（如果 EditorFontSizes41 是 SelectableList<string>）
            NuGet = selectable;

            NuGet.SelectionChanged += EditorFontSizes_SelectionChanged;
            // 1) 先绑定一次
            HookNuGetAndInit();

            // 2) 活动文档变了时，重新绑定
            _mainViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentOpenDocument))
                    HookNuGetAndInit();
            };

        }

        private void HookNuGetAndInit()
        {
            var newNuget = _mainViewModel.CurrentOpenDocument?.NuGet;
            if (ReferenceEquals(_nuget, newNuget)) return;

            if (_nuget != null) _nuget.PropertyChanged -= OnNuGetPropertyChanged;
            _nuget = newNuget;
            if (_nuget != null) _nuget.PropertyChanged += OnNuGetPropertyChanged;

            // 初始化/切换文档时立刻用当前包刷新一次
            UpdateEditorFontSizesFromNuGet();
        }

        private void OnNuGetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NuGetDocumentViewModel.Packages))
            {
                // 注意：NuGet 的搜索回调可能在后台线程触发
                Dispatcher.UIThread.Post(UpdateEditorFontSizesFromNuGet);
            }
        }

        private void UpdateEditorFontSizesFromNuGet()
        {
            var names = _nuget?.Packages?.Select(p => p.Id) ?? Enumerable.Empty<string>();
            NuGet = new SelectableList<string>(names);
        }

        private void EditorFontSizes_SelectionChanged(object? sender, EventArgs e)
        {            
            Console.WriteLine("SelectedValue running");
            // 优先使用触发事件的实例（sender），避免因属性引用被替换而读取到旧实例或 null
            var list = sender as PropertyModels.Collections.SelectableList<string> ?? NuGet;

            // 仍做空检查并打印调试信息
            var selected = list?.SelectedValue;
            // 找到当前文档的 NuGet view model（可能为 null）
            var nuget = _mainViewModel.CurrentOpenDocument?.NuGet;
            var selectedId = list?.SelectedValue;
            if (nuget == null)
            {
                Console.WriteLine("No NuGetDocumentViewModel available for current document.");
                return;
            }

            // 在包列表中查找匹配的 PackageData（以 Id 为匹配键）
            var package = nuget.Packages?.FirstOrDefault(p => string.Equals(p.Id, selectedId, StringComparison.OrdinalIgnoreCase));
            if (package == null)
            {
                Console.WriteLine($"No package found with Id '{selectedId}'.");
                return;
            }
            var cmd = nuget.InstallPackageCommand;
             _mainViewModel.CurrentOpenDocument?.NuGet.InstallPackage(package);
            //_mainViewModel.CurrentOpenDocument.NuGet.InstallPackageCommand;
        }



        [Category("leftMenuItem60")]
        [DisplayName("leftMenuItem61")]
        [Description("当前 FFT Provider（只读显示）")]
        [ControlClasses("readonly")]
        // 如果需要在属性面板上编辑它，去掉 [ReadOnly(true)] 注解；下面实现为可写以实现双向绑定
        public string? SearchTerm
        {
#pragma warning disable CA1305 // 指定 IFormatProvider
            get => _mainViewModel.CurrentOpenDocument?.NuGet.SearchTerm;
#pragma warning restore CA1305 // 指定 IFormatProvider
            set
            {
                if (_mainViewModel.CurrentOpenDocument?.NuGet is { } nuget)
                {
                    nuget.SearchTerm = value;
                    Console.WriteLine(nuget.SearchTerm);

                    

                    //var packages = _mainViewModel.CurrentOpenDocument?.NuGet?.Packages ?? Array.Empty<PackageData>();

                    //// 将 PackageData 投影成 string，比如用 Id（也可以用 $"{p.Id} {p.Version}"）
                    //var names = packages.Select(p => p.Id);

                    //// 创建 SelectableList<string>
                    //var selectable = new SelectableList<string>(names);

                    //// 直接赋值（如果 EditorFontSizes41 是 SelectableList<string>）
                    //EditorFontSizes31 = selectable;
                }

            }
        }

        [Category("leftMenuItem60")]
        [DisplayName("leftMenuItem62")]
        [Description("Encrypt data for security")]
        public bool Prerelease
        {
            get => _mainViewModel.CurrentOpenDocument?.NuGet?.Prerelease ?? false;
            set
            {
                var nuget = _mainViewModel.CurrentOpenDocument?.NuGet;
                if (nuget == null) return;
                if (nuget.Prerelease == value) return;
                nuget.Prerelease = value;
                // 如果需要通知 UI：
                // OnPropertyChanged(nameof(LeftMenuItem62))
                Console.WriteLine("Prerelease");
                Console.WriteLine(nuget.Prerelease);
            }
        }


        [Category("leftMenuItem60")]
        [DisplayName("leftMenuItem63")]
        [Description("Please Select login name from list")]
        public SelectableList<string> NuGet
        {
            get => _editorFontSizes31;
            set
            {
                // 如果是同一个实例，确保订阅存在并返回
                if (ReferenceEquals(_editorFontSizes31, value))
                {
                    // 保证已订阅（可能尚未订阅）
                    if (_editorFontSizes31 != null)
                    {
                        _editorFontSizes31.SelectionChanged -= EditorFontSizes_SelectionChanged; // 安全退订（若未订阅会忽略）
                        _editorFontSizes31.SelectionChanged += EditorFontSizes_SelectionChanged;
                    }
                    RaisePropertyChanged(nameof(NuGet));
                    return;
                }

                // 退订旧实例
                if (_editorFontSizes31 != null)
                {
                    try { _editorFontSizes31.SelectionChanged -= EditorFontSizes_SelectionChanged; } catch { }
                }

                // 赋新实例（防 null）
                _editorFontSizes31 = value ?? new SelectableList<string>(Array.Empty<string>());

                // 在新实例上订阅
                _editorFontSizes31.SelectionChanged += EditorFontSizes_SelectionChanged;

                RaisePropertyChanged(nameof(NuGet));
            }
        }
        private SelectableList<string> _editorFontSizes31 = new SelectableList<string>(Array.Empty<string>());

        

        [Category("leftMenuItem64")]
        [DisplayName("leftMenuItem65")]
        [PathBrowsable(Filters = "Import Files(*.dll;*.png;*.bmp;*.tag)|*.dll;*.png;*.bmp;*.tag")]
        [Watermark("Import Path")]
        [Description("Select an image file")]
        public string ImportPath
        {
            get
            {
                return _mainViewModel.SelectedDllPath;
            }

            set
            {
                if (_mainViewModel.SelectedDllPath != value)
                {
                    _mainViewModel.SelectedDllPath = value;

                }
                Debug.WriteLine("11111");
                Debug.WriteLine(value);
            }
        }

        [Category("leftMenuItem64")]
        [DisplayName("leftMenuItem67")]
        [PropertyButton("Import", MethodName = "ImportDllButton_Click")]
        public int Import { get; set; } = 10;





    }


}
