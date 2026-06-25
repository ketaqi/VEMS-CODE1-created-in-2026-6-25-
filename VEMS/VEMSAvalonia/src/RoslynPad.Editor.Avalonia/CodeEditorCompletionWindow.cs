namespace RoslynPad.Editor;

/// <summary>
/// 代码编辑器自动补全窗口控件
/// 处理补全列表的样式和交互逻辑
/// </summary>
partial class CodeEditorCompletionWindow
{
    /// <summary>
    /// 初始化补全窗口的样式和事件
    /// 设置补全列表边框厚度，绑定鼠标按下事件取消软选择状态
    /// </summary>
    partial void Initialize()
    {
        CompletionList.ListBox.BorderThickness = new Thickness(1);
        CompletionList.ListBox.PointerPressed += (o, e) => _isSoftSelectionActive = false;
    }
}
