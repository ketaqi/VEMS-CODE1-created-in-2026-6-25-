using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Path = Avalonia.Controls.Shapes.Path;

namespace RoslynPad.Controls;

/// <summary>
/// Ribbon 控件：基于 <see cref="TabControl"/> 实现的 Office 风格功能区控件。
/// </summary>
/// <remarks>
/// <para>
/// 此控件提供类似 Microsoft Office 的 Ribbon 界面，具有以下功能：
/// <list type="bullet">
///   <item><description>支持折叠/展开（双击标签切换；单击在折叠时临时展开）</description></item>
///   <item><description>点击 Ribbon 外部区域时自动折叠</description></item>
///   <item><description>集成标题栏按钮：最小化、最大化/还原、关闭</description></item>
///   <item><description>标题拖拽区域支持：双击最大化/还原，拖动移动窗口</description></item>
/// </list>
/// </para>
/// <para>
/// 模板契约（TemplatePart）：
/// <list type="bullet">
///   <item><description><c>PART_HeaderPresenter</c> - Tab 标签头部区域</description></item>
///   <item><description><c>PART_TitleDragArea</c> - 窗口拖动区域</description></item>
///   <item><description><c>MaxIcon</c> - 最大化图标（Path）</description></item>
///   <item><description><c>RestoreIcon</c> - 还原图标（Canvas）</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:Ribbon SelectedIndex="{Binding SelectedTabIndex}"&gt;
///     &lt;controls:RibbonTab Header="文件"&gt;
///         &lt;controls:RibbonGroup Header="新建"&gt;
///             &lt;controls:RibbonButton Content="新建文件" Icon="{StaticResource NewIcon}" /&gt;
///         &lt;/controls:RibbonGroup&gt;
///     &lt;/controls:RibbonTab&gt;
/// &lt;/controls:Ribbon&gt;
/// </code>
/// </example>
public class Ribbon : TabControl
{
    /// <summary>最大化图标元素。</summary>
    private Path? _maxIcon;

    /// <summary>还原图标元素。</summary>
    private Canvas? _restoreIcon;

    /// <summary>宿主窗口引用。</summary>
    private Window? _hostWindow;

    #region 依赖属性

    /// <summary>
    /// 标识 <see cref="IsCollapsed"/> 依赖属性。
    /// </summary>
    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<Ribbon, bool>(nameof(IsCollapsed));

    /// <summary>
    /// 标识 <see cref="MinimizeCommand"/> 依赖属性。
    /// </summary>
    public static readonly StyledProperty<ICommand?> MinimizeCommandProperty =
        AvaloniaProperty.Register<Ribbon, ICommand?>(nameof(MinimizeCommand));

    /// <summary>
    /// 标识 <see cref="MaximizeCommand"/> 依赖属性。
    /// </summary>
    public static readonly StyledProperty<ICommand?> MaximizeCommandProperty =
        AvaloniaProperty.Register<Ribbon, ICommand?>(nameof(MaximizeCommand));

    /// <summary>
    /// 标识 <see cref="CloseCommand"/> 依赖属性。
    /// </summary>
    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<Ribbon, ICommand?>(nameof(CloseCommand));

    /// <summary>
    /// 获取或设置 Ribbon 是否处于折叠状态。
    /// </summary>
    /// <value>
    /// <c>true</c> 表示 Ribbon 已折叠，仅显示标签；
    /// <c>false</c> 表示 Ribbon 展开，显示完整内容。
    /// </value>
    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    /// <summary>
    /// 获取或设置最小化窗口的命令。
    /// </summary>
    public ICommand? MinimizeCommand
    {
        get => GetValue(MinimizeCommandProperty);
        set => SetValue(MinimizeCommandProperty, value);
    }

    /// <summary>
    /// 获取或设置最大化/还原窗口的命令。
    /// </summary>
    public ICommand? MaximizeCommand
    {
        get => GetValue(MaximizeCommandProperty);
        set => SetValue(MaximizeCommandProperty, value);
    }

    /// <summary>
    /// 获取或设置关闭窗口的命令。
    /// </summary>
    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    #endregion

    #region 模板应用

    /// <summary>
    /// 应用控件模板时调用：查找模板元素并初始化窗口状态监听。
    /// </summary>
    /// <param name="e">模板应用事件参数。</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 从模板作用域查找图标元素
        _maxIcon = e.NameScope.Find<Path>("MaxIcon");
        _restoreIcon = e.NameScope.Find<Canvas>("RestoreIcon");

        // 获取宿主窗口并监听状态变化
        _hostWindow = this.GetVisualRoot() as Window;
        if (_hostWindow != null)
        {
            SyncMaxIcons(_hostWindow.WindowState);
            _hostWindow.GetObservable(Window.WindowStateProperty)
                       .Subscribe(state => SyncMaxIcons(state));
        }
    }

    /// <summary>
    /// 根据窗口状态同步最大化/还原图标的可见性。
    /// </summary>
    /// <param name="state">当前窗口状态。</param>
    private void SyncMaxIcons(WindowState state)
    {
        if (_maxIcon != null) _maxIcon.IsVisible = state != WindowState.Maximized;
        if (_restoreIcon != null) _restoreIcon.IsVisible = state == WindowState.Maximized;
    }

    #endregion

    #region 构造函数与初始化

    /// <summary>
    /// 静态构造函数：设置默认属性值。
    /// </summary>
    static Ribbon()
    {
        // Ribbon 本身不需要获得焦点（把焦点留给内部元素）
        FocusableProperty.OverrideDefaultValue<Ribbon>(false);
    }

    /// <summary>
    /// 指定样式查找键为 <see cref="Ribbon"/>。
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Ribbon);

    /// <summary>外部点击处理器引用。</summary>
    private EventHandler<PointerPressedEventArgs>? _outsideHandler;

    /// <summary>双击判定延时。</summary>
    private readonly TimeSpan _doubleClickDelay = TimeSpan.FromMilliseconds(250);

    /// <summary>单击等待标志。</summary>
    private bool _singleClickPending;

    /// <summary>
    /// 初始化 <see cref="Ribbon"/> 类的新实例。
    /// </summary>
    /// <remarks>
    /// 注册指针事件处理器，并为标题栏按钮提供默认命令实现。
    /// </remarks>
    public Ribbon()
    {
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);

        // 默认窗口控制命令
        MinimizeCommand = new RelayCommand(_ =>
        {
            if (this.GetVisualRoot() is Window wnd)
                wnd.WindowState = WindowState.Minimized;
        });

        MaximizeCommand = new RelayCommand(_ =>
        {
            if (this.GetVisualRoot() is Window wnd)
            {
                wnd.WindowState = wnd.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        });

        CloseCommand = new RelayCommand(_ =>
        {
            (this.GetVisualRoot() as Window)?.Close();
        });
    }

    #endregion

    #region 指针事件处理

    /// <summary>
    /// 处理指针按下事件：实现折叠/展开、窗口拖动和双击最大化等交互。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="e">指针事件参数。</param>
    private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var wnd = this.GetVisualRoot() as Window;
        if (wnd == null) return;

        // 不拦截按钮点击
        if (e.Source is Button) return;

        // Tab 标题区域
        if (e.Source is TextBlock || (e.Source as Control)?.Name == "PART_HeaderPresenter")
        {
            if (e.ClickCount == 2)
            {
                _singleClickPending = false;
                IsCollapsed = !IsCollapsed;
            }
            else if (e.ClickCount == 1)
            {
                _singleClickPending = true;
                await Task.Delay(_doubleClickDelay).ConfigureAwait(true);
                if (_singleClickPending)
                {
                    if (IsCollapsed)
                    {
                        IsCollapsed = false;
                        RegisterOutsideClick();
                    }
                }
            }
            return;
        }

        // 标题拖动区域
        if ((e.Source as Control)?.Name == "PART_TitleDragArea")
        {
            if (e.ClickCount == 2)
            {
                wnd.WindowState = wnd.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else if (e.ClickCount == 1 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                wnd.BeginMoveDrag(e);
            }
            return;
        }
    }

    /// <summary>
    /// 注册外部点击监听：点击 Ribbon 外部时自动折叠。
    /// </summary>
    private void RegisterOutsideClick()
    {
        var top = this.GetVisualRoot() as TopLevel;
        if (top == null) return;

        _outsideHandler = (s, e) =>
        {
            var p = e.GetPosition(this);
            var rect = new Rect(this.Bounds.Size);

            if (!rect.Contains(p))
            {
                IsCollapsed = true;
                top.PointerPressed -= _outsideHandler;
                _outsideHandler = null;
            }
        };

        top.PointerPressed += _outsideHandler;
    }

    #endregion
}

/// <summary>
/// 简化版命令实现：用于 Ribbon 标题栏按钮的命令绑定。
/// </summary>
/// <remarks>
/// 此类提供最小化的 <see cref="ICommand"/> 实现，不支持 <see cref="CanExecuteChanged"/> 事件。
/// 适用于简单的按钮命令场景。
/// </remarks>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// 初始化 <see cref="RelayCommand"/> 类的新实例。
    /// </summary>
    /// <param name="execute">执行委托。</param>
    /// <param name="canExecute">可选的可执行判断委托。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="execute"/> 为 <c>null</c> 时抛出。</exception>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// 此实现不转发 <see cref="CanExecuteChanged"/> 事件。
    /// </summary>
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    /// <inheritdoc/>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc/>
    public void Execute(object? parameter) => _execute(parameter);
}
