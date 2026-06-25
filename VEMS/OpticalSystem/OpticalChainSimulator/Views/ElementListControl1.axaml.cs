using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpticalChainSimulator.ViewModels;
using System;
using System.Diagnostics; // 补充Debug引用（原代码使用了Debug.WriteLine）

namespace OpticalChainSimulator.Views
{
    /// <summary>
    /// 光学元件列表控件
    /// 负责展示元件列表、处理元件参数编辑交互、拦截指针事件阻止误拖拽、双击切换元件展开/折叠状态
    /// </summary>
    public partial class ElementListControl1 : UserControl
    {
        /// <summary>
        /// 主窗口视图模型实例（用于绑定数据和执行命令）
        /// </summary>
        private MainWindowViewModel _vm;

        /// <summary>
        /// 列表控件中嵌套的ListBox实例（延迟初始化，在Loaded事件中查找）
        /// </summary>
        private ListBox? _elementsListBox;

        /// <summary>
        /// 控件构造函数
        /// 初始化控件资源、注册指针事件、绑定ViewModel、监听控件加载事件
        /// </summary>
        public ElementListControl1()
        {
            // 初始化控件XAML资源
            InitializeComponent();

            // 核心：注册PointerPressed事件（隧道路由策略），在ContextDragBehavior之前拦截，避免编辑时误触发拖拽
            this.AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);

            // 双击功能相关：初始化ViewModel并绑定数据上下文
            _vm = new MainWindowViewModel(null);
            DataContext = _vm;

            // 监听控件加载完成事件（用于查找嵌套的ListBox并绑定双击事件）
            this.Loaded += ElementListControl1_Loaded;
        }

        /// <summary>
        /// 初始化控件XAML资源
        /// </summary>
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        /// <summary>
        /// 快捷获取控件内的ElementsListBox实例
        /// </summary>
        public ListBox InnerListBox => this.FindControl<ListBox>("ElementsListBox");

        /// <summary>
        /// 参数文本框失去焦点事件处理方法
        /// 触发ViewModel刷新选中元件的参数
        /// </summary>
        /// <param name="sender">事件发送者（文本框）</param>
        /// <param name="e">路由事件参数</param>
        private void OnParameterTextBoxLostFocus(object? sender, RoutedEventArgs e)
        {
            // 数据上下文为MainWindowViewModel时，执行参数刷新
            if (this.DataContext is MainWindowViewModel mwvm)
                mwvm.RefreshSelectedParameters();
        }

        /// <summary>
        /// 参数文本框按键按下事件处理方法
        /// 按下Enter键时刷新参数（替代失去焦点操作），并标记事件已处理
        /// </summary>
        /// <param name="sender">事件发送者（文本框）</param>
        /// <param name="e">按键事件参数</param>
        private void OnParameterTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            // 按下Enter键且数据上下文有效时，刷新参数并阻止事件冒泡
            if (e.Key == Key.Enter && this.DataContext is MainWindowViewModel mwvm)
            {
                mwvm.RefreshSelectedParameters();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 隧道阶段拦截PointerPressed事件
        /// 核心逻辑：点击交互控件（TextBox/Button等）时，阻止ContextDragBehavior启动拖拽，避免编辑时误拖拽
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">指针按压事件参数</param>
        private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var source = e.Source as Control;
            if (source == null) return;

            // 向上遍历可视化树，判断点击的控件类型
            var current = source;
            while (current != null)
            {
                // 点击按钮/下拉框等交互控件时，不拦截（允许正常操作）
                if (current is ToggleButton || current is Button || current is ComboBox)
                {
                    return;
                }

                // 点击文本框时，特殊处理：阻止拖拽并手动聚焦
                if (current is TextBox)
                {
                    // 检查文本框是否在元件展开区域内
                    var parent = current.Parent as Control;
                    while (parent != null)
                    {
                        if (parent.DataContext is OpticalElementViewModel vm && vm.IsExpanded)
                        {
                            // 标记事件已处理，阻止拖拽逻辑触发
                            e.Handled = true;

                            // 手动聚焦文本框并将光标定位到文本末尾
                            var textBox = (source as TextBox) ?? (current as TextBox);
                            if (textBox != null)
                            {
                                textBox.Focus();

                                // 延迟设置光标位置（确保聚焦完成后执行）
                                Dispatcher.UIThread.Post(() =>
                                {
                                    textBox.CaretIndex = textBox.Text?.Length ?? 0;
                                }, DispatcherPriority.Input);
                            }
                            return;
                        }
                        parent = parent.Parent as Control;
                    }
                    return;
                }
                current = current.Parent as Control;
            }
        }

        /// <summary>
        /// 已废弃：展开区域指针按压事件处理方法
        /// 原逻辑已迁移到OnPreviewPointerPressed，此方法留空仅做兼容
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">指针按压事件参数</param>
        private void OnExpandedAreaPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // 此方法已废弃，逻辑合并到OnPreviewPointerPressed中
        }

        #region 双击事件处理：切换元件展开/折叠状态
        /// <summary>
        /// 控件加载完成事件处理方法
        /// 核心：查找嵌套的ElementsListBox并绑定双击事件（确保可视化树完全构建后查找）
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">路由事件参数</param>
        private void ElementListControl1_Loaded(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("===== 开始查找ElementsListBox =====");

            // 关键：手动查找XAML中命名为ElementsListBox的ListBox控件
            _elementsListBox = this.FindControl<ListBox>("ElementsListBox");

            // 调试输出：验证查找结果
            Debug.WriteLine($"手动查找后 ElementsListBox 是否为null: {_elementsListBox == null}");

            if (_elementsListBox != null)
            {
                // 绑定ListBox的双击事件（适配Avalonia 11.x的DoubleTapped事件）
                _elementsListBox.DoubleTapped += ElementsListBox_DoubleTapped;
            }
            else
            {
                Debug.WriteLine("错误：未找到ElementsListBox，请检查XAML中的x:Name是否为ElementsListBox");
                // 兜底方案：延迟查找（确保可视化树完全加载）
                Dispatcher.UIThread.Post(() =>
                {
                    _elementsListBox = this.FindControl<ListBox>("ElementsListBox");
                    Debug.WriteLine($"延迟查找后 ElementsListBox 是否为null: {_elementsListBox == null}");
                    if (_elementsListBox != null)
                    {
                        _elementsListBox.DoubleTapped += ElementsListBox_DoubleTapped;
                    }
                });
            }
        }

        /// <summary>
        /// ListBox双击事件处理方法
        /// 核心逻辑：切换选中元件的IsExpanded状态（展开 ↔ 折叠）
        /// </summary>
        /// <param name="sender">事件发送者（ListBox）</param>
        /// <param name="e">双击事件参数</param>
        private void ElementsListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            Debug.WriteLine("===== 双击事件触发 =====");

            // 基础空值校验：ViewModel/ListBox未初始化时直接返回
            if (_vm == null || _elementsListBox == null)
            {
                Debug.WriteLine("错误：ViewModel或ListBox未初始化");
                return;
            }
            if (_vm.PrintElementsInfoCommand == null)
            {
                Debug.WriteLine("错误：PrintElementsInfoCommand未定义");
                return;
            }

            // 获取ListBox选中的元件ViewModel（做空值校验）
            var selectedElement = _elementsListBox.SelectedItem as OpticalElementViewModel;
            Debug.WriteLine($"选中项：{selectedElement?.Name ?? "null"}");

            if (selectedElement == null)
            {
                Debug.WriteLine("错误：未选中任何元素，无法切换展开状态");
                return;
            }

            // 同步选中项到ViewModel（确保ViewModel与ListBox选中状态一致）
            _vm.SelectedElement = selectedElement;

            // 核心：切换元件的展开/折叠状态（取反当前值）
            selectedElement.IsExpanded = !selectedElement.IsExpanded;

            // 调试输出：记录状态变更
            Debug.WriteLine($"元素 {selectedElement.Name} 的展开状态已切换为：{selectedElement.IsExpanded}");

            // 可选：执行打印元件信息命令（如需启用，取消注释）
            // _vm.PrintElementsInfoCommand.Execute(null);
        }
        #endregion
    }
}