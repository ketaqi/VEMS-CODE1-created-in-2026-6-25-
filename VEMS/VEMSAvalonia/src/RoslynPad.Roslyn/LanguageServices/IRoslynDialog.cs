namespace RoslynPad.Roslyn.LanguageServices;

/// <summary>
/// 定义 Roslyn 对话框的核心接口，规范对话框的视图模型管理和显示行为
/// </summary>
internal interface IRoslynDialog
{
    /// <summary>
    /// 获取或设置对话框关联的视图模型对象
    /// </summary>
    object ViewModel { get; set; }

    /// <summary>
    /// 显示对话框，并返回对话框的关闭结果
    /// </summary>
    /// <returns>
    /// 一个可空的布尔值，表示对话框的确认/取消状态：
    /// <list type="bullet">
    /// <item><c>true</c> - 对话框被确认（如点击确定按钮）</item>
    /// <item><c>false</c> - 对话框被取消（如点击取消按钮）</item>
    /// <item><c>null</c> - 对话框被关闭（无确认/取消操作）</item>
    /// </list>
    /// </returns>
    bool? Show();
}
