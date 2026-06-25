using RoslynPad.Build;
using RoslynPad.Utilities;

namespace RoslynPad.UI;

/// <summary>
/// 结果视图模型接口：定义运行结果展示区域的数据和操作契约。
/// </summary>
/// <remarks>
/// <para>
/// 此接口定义了代码执行结果展示区域的核心功能：
/// <list type="bullet">
///   <item><description>存储和展示执行结果集合</description></item>
///   <item><description>提供结果的文本表示形式</description></item>
///   <item><description>支持复制和清除结果的命令</description></item>
///   <item><description>控制结果区域的字体样式</description></item>
/// </list>
/// </para>
/// <para>
/// 实现类包括：
/// <list type="bullet">
///   <item><description><c>GlobalResultsViewModel</c> - 全局共享的结果视图</description></item>
///   <item><description><c>OpenDocumentViewModel</c> - 单个文档的结果视图</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 在视图中绑定
/// &lt;ItemsControl ItemsSource="{Binding Results}" /&gt;
/// &lt;Button Command="{Binding CopyAllResultsCommand}" /&gt;
/// &lt;Button Command="{Binding ClearResultsCommand}" /&gt;
/// </code>
/// </example>
public interface IResultsViewModel
{
    /// <summary>
    /// 获取所有执行结果的集合。
    /// </summary>
    /// <value>
    /// 包含所有 <see cref="IResultObject"/> 实例的可枚举集合。
    /// </value>
    /// <remarks>
    /// 此集合通常绑定到 <c>ItemsControl</c> 或类似的列表控件。
    /// </remarks>
    IEnumerable<IResultObject> Results { get; }

    /// <summary>
    /// 获取所有结果的纯文本表示。
    /// </summary>
    /// <value>
    /// 将所有结果格式化后的字符串，适用于复制到剪贴板或保存到文件。
    /// </value>
    string ResultsText { get; }

    /// <summary>
    /// 获取复制所有结果到剪贴板的命令。
    /// </summary>
    /// <value>
    /// 执行时将 <see cref="ResultsText"/> 复制到系统剪贴板。
    /// </value>
    IDelegateCommand CopyAllResultsCommand { get; }

    /// <summary>
    /// 获取清除所有结果的命令。
    /// </summary>
    /// <value>
    /// 执行时清空 <see cref="Results"/> 集合。
    /// </value>
    IDelegateCommand ClearResultsCommand { get; }

    /// <summary>
    /// 获取结果区域使用的字体名称。
    /// </summary>
    /// <value>
    /// 等宽字体名称，如 "Consolas"、"Fira Code"。
    /// </value>
    string EditorFontFamily1 { get; }

    /// <summary>
    /// 获取结果区域的字号。
    /// </summary>
    /// <value>
    /// 字体大小（以磅为单位）。
    /// </value>
    double FontSize { get; }

    /// <summary>
    /// 添加一个结果对象到集合中。
    /// </summary>
    /// <param name="o">要添加的结果对象。</param>
    /// <remarks>
    /// 此方法应确保线程安全，因为结果可能从后台线程添加。
    /// 实现类通常会封送到 UI 线程执行实际的集合修改。
    /// </remarks>
    void AddResult(IResultObject o);

    /// <summary>
    /// 获取本地化管理器实例。
    /// </summary>
    /// <value>
    /// 用于获取本地化字符串的 <see cref="LocalizationManager"/> 单例。
    /// </value>
    public LocalizationManager Localized => LocalizationManager.Instance;
}
