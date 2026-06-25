// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable

namespace RoslynPad.Editor;

/// <summary>
/// 上下文动作渲染器，用于在编辑器中显示上下文动作灯泡及对应的上下文菜单
/// </summary>
public sealed class ContextActionsRenderer
{
    /// <summary>
    /// 光标移动后延迟显示灯泡的毫秒数
    /// </summary>
    private const int DelayMoveMilliseconds = 500;

    private readonly ObservableCollection<IContextActionProvider> _providers;
    private readonly CodeTextEditor _editor;
    private readonly TextMarkerService _textMarkerService;
    private readonly MarkerMargin _bulbMargin;
    private readonly DispatcherTimer _delayMoveTimer;
    private readonly ContextActionsBulbContextMenu _contextMenu;

    private CancellationTokenSource? _cancellationTokenSource;
    private List<object>? _actions;
    private ImageSource? _iconImage;

    /// <summary>
    /// 初始化<see cref="ContextActionsRenderer"/>类的新实例
    /// </summary>
    /// <param name="editor">关联的代码文本编辑器</param>
    /// <param name="textMarkerService">文本标记服务</param>
    /// <exception cref="ArgumentNullException">当<paramref name="editor"/>为null时抛出</exception>
    public ContextActionsRenderer(CodeTextEditor editor, TextMarkerService textMarkerService)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _textMarkerService = textMarkerService;

        _contextMenu = CreateContextMenu();
        _bulbMargin = new MarkerMargin { Width = 16, Margin = new Thickness(0, 0, 5, 0) };
        _bulbMargin.MarkerPointerDown += (o, e) => OpenContextMenu();
        var index = editor.TextArea.LeftMargins.Count > 0 ? editor.TextArea.LeftMargins.Count - 1 : 0;
        editor.TextArea.LeftMargins.Insert(index, _bulbMargin);

        editor.TextArea.Caret.PositionChanged += CaretPositionChanged;

        editor.KeyDown += ContextActionsRenderer_KeyDown;
        _providers = [];
        _providers.CollectionChanged += Providers_CollectionChanged;

        editor.TextArea.TextView.ScrollOffsetChanged += ScrollChanged;
        _delayMoveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DelayMoveMilliseconds) };
        _delayMoveTimer.Stop();
        _delayMoveTimer.Tick += TimerMoveTick;
    }

    /// <summary>
    /// 获取或设置灯泡图标
    /// </summary>
    public ImageSource? IconImage
    {
        get => _iconImage;
        set
        {
            _bulbMargin.MarkerImage = value;
            _iconImage = value;
        }
    }

    /// <summary>
    /// 获取上下文动作提供器列表
    /// </summary>
    public IList<IContextActionProvider> Providers => _providers;

    /// <summary>
    /// 上下文动作提供器集合变更时启动延迟定时器
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">集合变更事件参数</param>
    private void Providers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => StartTimer();

    /// <summary>
    /// 处理键盘按下事件（Ctrl+.触发上下文动作）
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">键盘事件参数</param>
    private async void ContextActionsRenderer_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!(e.Key == Key.OemPeriod && e.HasModifiers(ModifierKeys.Control))) return;

        Cancel();
        if (!await LoadActionsWithCancellationAsync().ConfigureAwait(true) || _actions?.Count < 1)
        {
            HideBulb();
            return;
        }

        _contextMenu.ItemsSource = _actions!;
        _bulbMargin.LineNumber = _editor.TextArea.Caret.Line;
        OpenContextMenu();
    }

    /// <summary>
    /// 打开上下文动作菜单
    /// </summary>
    private void OpenContextMenu()
    {
        _contextMenu.Open(_bulbMargin.Marker);
    }

    /// <summary>
    /// 创建上下文动作菜单
    /// </summary>
    /// <returns>上下文动作菜单实例</returns>
    private ContextActionsBulbContextMenu CreateContextMenu()
    {
        var contextMenu = new ContextActionsBulbContextMenu(new ActionCommandConverter(GetActionCommand));

        // TODO: workaround to refresh menu with latest document
#if AVALONIA
        contextMenu.Opening
#else
        contextMenu.ContextMenuOpening
#endif
            += async (sender, args) =>
            {
                if (await LoadActionsWithCancellationAsync().ConfigureAwait(true))
                {
                    if (sender is ContextActionsBulbContextMenu menu)
                    {
                        menu.ItemsSource = _actions;
                    }
                }
            };

        return contextMenu;
    }

    /// <summary>
    /// 带取消机制的上下文动作加载方法
    /// </summary>
    /// <returns>加载成功返回true，否则返回false</returns>
    private async Task<bool> LoadActionsWithCancellationAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            _actions = await LoadActionsAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            // 忽略加载过程中的异常
        }
        _cancellationTokenSource = null;
        return false;
    }

    /// <summary>
    /// 获取指定动作对应的命令
    /// </summary>
    /// <param name="action">上下文动作实例</param>
    /// <returns>动作对应的命令，无匹配时返回null</returns>
    private ICommand? GetActionCommand(object action) =>
        _providers.Select(provider => provider.GetActionCommand(action))
            .FirstOrDefault(command => command != null);

    /// <summary>
    /// 异步加载上下文动作列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上下文动作列表</returns>
    private async Task<List<object>> LoadActionsAsync(CancellationToken cancellationToken)
    {
        var allActions = new List<object>();
        foreach (var provider in _providers)
        {
            var offset = _editor.TextArea.Caret.Offset;
            var length = 0;
            var marker = _textMarkerService.GetMarkersAtOffset(offset).FirstOrDefault();
            if (marker != null)
            {
                offset = marker.StartOffset;
                length = marker.Length;
            }
            var actions = await provider.GetActions(offset, length, cancellationToken).ConfigureAwait(true);
            allActions.AddRange(actions);
        }
        return allActions;
    }

    /// <summary>
    /// 编辑器滚动时启动延迟定时器
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private void ScrollChanged(object? sender, EventArgs e) => StartTimer();

    /// <summary>
    /// 延迟定时器触发时加载上下文动作并显示灯泡
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private async void TimerMoveTick(object? sender, EventArgs e)
    {
        if (!_delayMoveTimer.IsEnabled)
        {
            return;
        }

        Cancel();

        // 光标超出编辑器可视区域时隐藏灯泡
        var textView = _editor.TextArea.TextView;
        var editorRect = new Rect((Point)textView.ScrollOffset, textView.GetRenderSize());
        var caretRect = _editor.TextArea.Caret.CalculateCaretRectangle();
        if (!editorRect.Contains(caretRect))
        {
            HideBulb();
            return;
        }

        if (!await LoadActionsWithCancellationAsync().ConfigureAwait(true) || _actions?.Count < 1)
        {
            HideBulb();
            return;
        }

        _contextMenu.ItemsSource = _actions!;
        _bulbMargin.LineNumber = _editor.TextArea.Caret.Line;
    }

    /// <summary>
    /// 隐藏上下文动作灯泡
    /// </summary>
    private void HideBulb() => _bulbMargin.LineNumber = null;

    /// <summary>
    /// 光标位置变更时启动延迟定时器
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private void CaretPositionChanged(object? sender, EventArgs e) => StartTimer();

    /// <summary>
    /// 启动延迟定时器（仅当存在动作提供器时）
    /// </summary>
    private void StartTimer()
    {
        if (_providers.Count == 0)
        {
            return;
        }

        _delayMoveTimer.Start();
    }

    /// <summary>
    /// 取消当前的动作加载并停止延迟定时器
    /// </summary>
    private void Cancel()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
        }

        _delayMoveTimer.Stop();
    }
}

/// <summary>
/// 动作命令转换器，用于将上下文动作转换为对应的ICommand
/// </summary>
/// <param name="commandProvider">命令提供器委托</param>
internal class ActionCommandConverter(Func<object, ICommand?>? commandProvider) : IValueConverter
{
    /// <summary>
    /// 获取命令提供器委托
    /// </summary>
    public Func<object, ICommand?>? CommandProvider { get; } = commandProvider;

    /// <summary>
    /// 将动作转换为对应的命令
    /// </summary>
    /// <param name="value">动作实例</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>动作对应的命令，无匹配时返回null</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value == null ? null : CommandProvider?.Invoke(value);

    /// <summary>
    /// 反向转换（不支持）
    /// </summary>
    /// <param name="value">转换值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>不支持，始终抛出<see cref="NotSupportedException"/></returns>
    /// <exception cref="NotSupportedException">始终抛出</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
