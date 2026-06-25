
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
using RoslynPad.FeatureDemos.Views;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Models
{
    [AutoCollapseCategories("AutoCollapse")]
    public class SimpleObject2 : ReactiveObject
    {
#pragma warning disable CA1051 // 不要声明可见实例字段
        public readonly string Description;
#pragma warning restore CA1051 // 不要声明可见实例字段

        // 现在强制为非空
        private readonly MainViewModel _mainViewModel;
        // public IImage? AvaloniaBanner { get; set; }
        // 仅保留必须传入 MainViewModel 的构造函数
        public SimpleObject2(MainViewModel mainViewModel, string description)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            Description = description;

            // 初始化下拉数据源（字体名称）
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

            int coreCount = Environment.ProcessorCount;
            Console.WriteLine($"本机CPU核心数为: {coreCount}");
            Name1 = coreCount;


        }

        public override string ToString() => $"({GetHashCode()}){Description}";

        private int _Name1  ;

         [Category("leftMenuItem32")]
        [DisplayName("leftMenuItem321")]
        [Description("核心数")]
        [ReadOnly(true)]
        [ControlClasses("readonly")]
        public int Name1
        {
#pragma warning disable CA1305 // 指定 IFormatProvider
            get => _Name1;
#pragma warning restore CA1305 // 指定 IFormatProvider
            set
            {
                _Name1= value;
            }
        }

        // 只读显示属性（文本显示框）
        [Category("leftMenuItem32")]
        [DisplayName("leftMenuItem322")]
        [Description("FftLibraryInfo")]
        [ReadOnly(true)]
        [ControlClasses("readonly")]
        public string FftLibraryInfo => _mainViewModel.FftLibraryInfo;

        // 只读显示属性（文本显示框）
        [Category("leftMenuItem32")]
        [DisplayName("leftMenuItem323")]
        [Description("LapackLibraryInfo")]
        [ReadOnly(true)]
        [ControlClasses("readonly")]
        public string LapackLibraryInfo => _mainViewModel.LapackLibraryInfo;

        // 只读显示属性（文本显示框）
        [Category("leftMenuItem32")]
        [DisplayName("leftMenuItem324")]
        [Description("BlasLibraryInfo")]
        [ReadOnly(true)]
        [ControlClasses("readonly")]
        public string BlasLibraryInfo => _mainViewModel.BlasLibraryInfo;

        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem331")]
        [Description("测试次数")]
        [ControlClasses("readonly")]
        public string TestRunCount
        {
#pragma warning disable CA1305 // 指定 IFormatProvider
            get => _mainViewModel.TestRunCount.ToString();
#pragma warning restore CA1305 // 指定 IFormatProvider
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                // 尝试把输入解析为 int，然后写回主 VM
                if (int.TryParse(value, out var parsed))
                {
                    if (_mainViewModel.TestRunCount != parsed)
                    {
                        // 写回主 VM，会触发主 VM 的 PropertyChanged（我们已订阅，会反向通知 Name5）
                        _mainViewModel.TestRunCount = parsed;

                        // 可选：立即通知一次（通常主 VM 的 PropertyChanged 足以触发 UI 刷新）
                        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name5)));
                    }
                }
                else
                {
                    // 解析失败：选择忽略、回退或抛出/记录错误
                    // 例如：记录到 Debug 输出，或显示错误提示（PropertyGrid 里通常无弹窗）
                    System.Diagnostics.Debug.WriteLine($"Name5: 无法将 \"{value}\" 解析为 int，忽略设置。");
                }
            }
        }


        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem332")]
        [PropertyButton("RUN", MethodName = "ExecuteTestMatrixMultiply")]
        // 关键：让该行显示操作列
        public int ExecuteTestMatrixMultiply { get; set; } = 10;
        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem332")]
        [Description("Encrypt data for security")]
        public bool EncryptData
        {
            get => _mainViewModel.IsComplexMatrixMultiplyEnabled;
            set
            {
                if (_mainViewModel.IsComplexMatrixMultiplyEnabled != value)
                {
                    _mainViewModel.IsComplexMatrixMultiplyEnabled = value;
                    // 主 VM 设置会触发它自己的 PropertyChanged, 我们已经订阅它会将变更转发回本对象。
                    // 这里也可以主动通知一次，确保立即更新（通常冗余）：
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncryptData)));
                }
            }
        }


        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem333")]
        [PropertyButton("RUN", MethodName = "ExecuteTestMatrixMultiply1")]
        public int ExecuteTestMatrixMultiply1 { get; set; } = 10;

        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem333")]
        [Description("Encrypt data for security")]
        public bool EncryptData1
        {
            get => _mainViewModel.IsComplexLinearSolveEnabled;
            set
            {
                if (_mainViewModel.IsComplexLinearSolveEnabled != value)
                {
                    _mainViewModel.IsComplexLinearSolveEnabled = value;
                    // 主 VM 设置会触发它自己的 PropertyChanged, 我们已经订阅它会将变更转发回本对象。
                    // 这里也可以主动通知一次，确保立即更新（通常冗余）：
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncryptData)));
                }
            }
        }


        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem334")]
        [PropertyButton("RUN", MethodName = "ExecuteTestMatrixMultiply2")]

        public int ExecuteTestMatrixMultiply2 { get; set; } = 10;
        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem334")]
        [Description("Encrypt data for security")]
        public bool EncryptData2
        {
            get => _mainViewModel.IsComplexSvdEnabled;
            set
            {
                if (_mainViewModel.IsComplexSvdEnabled != value)
                {
                    _mainViewModel.IsComplexSvdEnabled = value;
                    // 主 VM 设置会触发它自己的 PropertyChanged, 我们已经订阅它会将变更转发回本对象。
                    // 这里也可以主动通知一次，确保立即更新（通常冗余）：
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncryptData)));
                }
            }
        }
        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem335")]
        [PropertyButton("RUN", MethodName = "ExecuteTestMatrixMultiply3")]

        public int ExecuteTestMatrixMultiply3 { get; set; } = 10;
        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem335")]
        [Description("Encrypt data for security")]
        public bool EncryptData3
        {
            get => _mainViewModel.IsComplexEigenEnabled;
            set
            {
                if (_mainViewModel.IsComplexEigenEnabled != value)
                {
                    _mainViewModel.IsComplexEigenEnabled = value;
                    // 主 VM 设置会触发它自己的 PropertyChanged, 我们已经订阅它会将变更转发回本对象。
                    // 这里也可以主动通知一次，确保立即更新（通常冗余）：
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncryptData)));
                }
            }
        }

        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem336")]
        [PropertyButton("RUN", MethodName = "ExecuteTestMatrixMultiply4")]
        public int ExecuteTestMatrixMultiply4 { get; set; } = 10;


        [Category("leftMenuItem33")]
        [DisplayName("leftMenuItem336")]
        [Description("Encrypt data for security")]
        public bool EncryptData4
        {
            get => _mainViewModel.IsComplexFftEnabled;
            set
            {
                if (_mainViewModel.IsComplexFftEnabled != value)
                {
                    _mainViewModel.IsComplexFftEnabled = value;
                    // 主 VM 设置会触发它自己的 PropertyChanged, 我们已经订阅它会将变更转发回本对象。
                    // 这里也可以主动通知一次，确保立即更新（通常冗余）：
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncryptData)));
                }
            }
        }

  
    }


}
