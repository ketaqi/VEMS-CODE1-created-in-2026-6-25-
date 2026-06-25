using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using RoslynPad.UI;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// 编辑器设置视图模型，管理字体、字号等编辑器外观配置。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此视图模型提供编辑器和输出区域的外观设置，包括：
    /// <list type="bullet">
    ///   <item><description>编辑器字体和字号</description></item>
    ///   <item><description>输出区域字体和字号</description></item>
    ///   <item><description>系统可用字体列表</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 设置更改会自动保存并通过事件通知相关组件。
    /// </para>
    /// </remarks>
    public class EditorSettingsViewModel : NotificationObject
    {
        /// <summary>
        /// 应用程序设置值。
        /// </summary>
        private readonly IApplicationSettingsValues _settings;

        /// <summary>
        /// 设置变更回调。
        /// </summary>
        private readonly Action? _onSettingsChanged;

        /// <summary>
        /// 最小字体大小。
        /// </summary>
        public const double MinimumFontSize = 8;

        /// <summary>
        /// 最大字体大小。
        /// </summary>
        public const double MaximumFontSize = 72;

        /// <summary>
        /// 初始化 <see cref="EditorSettingsViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="settings">应用程序设置值。</param>
        /// <param name="onSettingsChanged">设置变更时的回调委托（可选）。</param>
        public EditorSettingsViewModel(IApplicationSettingsValues settings, Action? onSettingsChanged = null)
        {
            _settings = settings;
            _onSettingsChanged = onSettingsChanged;

            // 从设置加载初始值
            _editorFontSize = settings.EditorFontSize;
            _editorFontFamily = settings.EditorFontFamily;
            _outputFontSize = settings.OutputFontSize;
            _editorFontFamily1 = settings.EditorFontFamily1;

            // 初始化系统字体列表
            SystemFontFamilies = new ObservableCollection<FontFamily>(
                FontManager.Current.SystemFonts.OrderBy(f => f.Name));
        }

        #region 编辑器字号

        private int _editorFontSize;

        /// <summary>
        /// 获取或设置编辑器字体大小。
        /// </summary>
        /// <value>字体大小（8-72 之间的整数）。</value>
        public int EditorFontSize
        {
            get => _editorFontSize;
            set
            {
                if (!IsValidFontSize(value))
                {
                    return;
                }

                if (SetProperty(ref _editorFontSize, value))
                {
                    _settings.EditorFontSize = value;
                    EditorFontSizeChanged?.Invoke(value);
                    _onSettingsChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 当编辑器字体大小变更时触发。
        /// </summary>
        public event Action<double>? EditorFontSizeChanged;

        /// <summary>
        /// 验证字体大小是否在有效范围内。
        /// </summary>
        /// <param name="value">要验证的字体大小。</param>
        /// <returns>若有效返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public static bool IsValidFontSize(double value) =>
            value >= MinimumFontSize && value <= MaximumFontSize;

        #endregion

        #region 输出区字号

        private int _outputFontSize = 14;

        /// <summary>
        /// 获取或设置输出区域字体大小。
        /// </summary>
        /// <value>字体大小，默认为 14。</value>
        public int OutputFontSize
        {
            get => _outputFontSize;
            set
            {
                if (SetProperty(ref _outputFontSize, value))
                {
                    _onSettingsChanged?.Invoke();
                }
            }
        }

        #endregion

        #region 编辑器字体

        private string _editorFontFamily = "Consolas";

        /// <summary>
        /// 获取或设置编辑器字体系列名称。
        /// </summary>
        /// <value>字体系列名称，默认为 "Consolas"。</value>
        public string EditorFontFamily
        {
            get => _editorFontFamily;
            set
            {
                if (SetProperty(ref _editorFontFamily, value))
                {
                    EditorFontFamilyChanged?.Invoke(value);
                    _onSettingsChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 当编辑器字体系列变更时触发。
        /// </summary>
        public event Action<string>? EditorFontFamilyChanged;

        private FontFamily? _selectedFontFamilyObject;

        /// <summary>
        /// 获取或设置选中的字体对象。
        /// </summary>
        /// <value>选中的 <see cref="FontFamily"/> 对象。</value>
        /// <remarks>
        /// 设置此属性会自动更新 <see cref="EditorFontFamily"/>。
        /// </remarks>
        public FontFamily? SelectedFontFamilyObject
        {
            get => _selectedFontFamilyObject;
            set
            {
                if (SetProperty(ref _selectedFontFamilyObject, value) && value != null)
                {
                    EditorFontFamily = value.Name;
                }
            }
        }

        #endregion

        #region 输出区字体

        private string _editorFontFamily1 = "Consolas";

        /// <summary>
        /// 获取或设置输出区域字体系列名称。
        /// </summary>
        /// <value>字体系列名称，默认为 "Consolas"。</value>
        public string EditorFontFamily1
        {
            get => _editorFontFamily1;
            set
            {
                if (SetProperty(ref _editorFontFamily1, value))
                {
                    EditorFontFamily1Changed?.Invoke(value);
                    _onSettingsChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 当输出区域字体系列变更时触发。
        /// </summary>
        public event Action<string>? EditorFontFamily1Changed;

        private FontFamily? _selectedFontFamilyObject1;

        /// <summary>
        /// 获取或设置输出区域选中的字体对象。
        /// </summary>
        /// <value>选中的 <see cref="FontFamily"/> 对象。</value>
        /// <remarks>
        /// 设置此属性会自动更新 <see cref="EditorFontFamily1"/>。
        /// </remarks>
        public FontFamily? SelectedFontFamilyObject1
        {
            get => _selectedFontFamilyObject1;
            set
            {
                if (SetProperty(ref _selectedFontFamilyObject1, value) && value != null)
                {
                    EditorFontFamily1 = value.Name;
                }
            }
        }

        #endregion

        #region 系统字体列表

        /// <summary>
        /// 获取系统可用字体系列集合。
        /// </summary>
        /// <value>按名称排序的字体系列列表。</value>
        public ObservableCollection<FontFamily> SystemFontFamilies { get; }

        #endregion

        #region 同步方法

        /// <summary>
        /// 从系统字体列表中同步选中的字体对象。
        /// </summary>
        /// <remarks>
        /// 根据当前的字体系列名称在系统字体列表中查找对应的字体对象，
        /// 并设置到 <see cref="SelectedFontFamilyObject"/> 和 <see cref="SelectedFontFamilyObject1"/>。
        /// </remarks>
        public void SyncFontFamilyObjects()
        {
            SelectedFontFamilyObject = SystemFontFamilies.FirstOrDefault(f => f.Name == EditorFontFamily);
            SelectedFontFamilyObject1 = SystemFontFamilies.FirstOrDefault(f => f.Name == EditorFontFamily1);
        }

        #endregion
    }
}
