namespace RoslynPad.Editor;

/// <summary>
/// 代码编辑器的自动补全窗口，扩展基础补全窗口以支持软选择功能
/// </summary>
public partial class CodeEditorCompletionWindow : CompletionWindow
{
    /// <summary>
    /// 标记软选择是否激活
    /// </summary>
    private bool _isSoftSelectionActive;

    /// <summary>
    /// 按键按下事件参数缓存
    /// </summary>
    private KeyEventArgs? _keyDownArgs;

    /// <summary>
    /// 初始化<CodeEditorCompletionWindow>实例
    /// </summary>
    /// <param name="textArea">关联的文本区域</param>
    public CodeEditorCompletionWindow(TextArea textArea) : base(textArea)
    {
        _isSoftSelectionActive = true;
        CompletionList.SelectionChanged += CompletionListOnSelectionChanged;

        Initialize();
    }

    /// <summary>
    /// 部分初始化方法（供平台特定实现扩展）
    /// </summary>
    partial void Initialize();

    /// <summary>
    /// 处理补全列表选择变更事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">选择变更事件参数</param>
    private void CompletionListOnSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (!UseHardSelection &&
            _isSoftSelectionActive && _keyDownArgs?.Handled != true
            && args.AddedItems?.Count > 0)
        {
            CompletionList.SelectedItem = null;
        }
    }

    /// <summary>
    /// 重写按键按下事件处理逻辑
    /// </summary>
    /// <param name="e">按键事件参数</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        // 忽略Home/End键的默认处理
        if (e.Key == Key.Home || e.Key == Key.End) return;

        _keyDownArgs = e;

        base.OnKeyDown(e);

        SetSoftSelection(e);
    }

    /// <summary>
    /// 设置软选择状态
    /// </summary>
    /// <param name="e">路由事件参数</param>
    private void SetSoftSelection(RoutedEventArgs e)
    {
        if (e.Handled)
        {
            _isSoftSelectionActive = false;
        }
    }

    /// <summary>
    /// 获取或设置是否使用硬选择模式（禁用软选择）
    /// </summary>
    public bool UseHardSelection { get; set; }
}
