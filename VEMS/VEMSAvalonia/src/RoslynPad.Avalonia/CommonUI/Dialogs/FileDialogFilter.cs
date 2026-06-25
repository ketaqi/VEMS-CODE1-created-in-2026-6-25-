namespace RoslynPad.UI;

/// <summary>
/// 文件对话框过滤器：表示文件类型过滤器的数据结构。
/// </summary>
/// <remarks>
/// <para>
/// 用于在打开/保存文件对话框中过滤显示的文件类型。
/// 每个过滤器包含一个显示名称（Header）和一组文件扩展名。
/// </para>
/// <para>
/// 扩展名格式说明：
/// <list type="bullet">
///   <item><description>不需要包含星号或点号，如 "cs" 而非 "*.cs"</description></item>
///   <item><description>使用 "*" 表示所有文件类型</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 单个扩展名
/// var csFilter = new FileDialogFilter("C# 源文件", "cs");
/// 
/// // 多个扩展名
/// var scriptFilter = new FileDialogFilter("C# 脚本", "cs", "csx");
/// 
/// // 所有文件
/// var allFilter = new FileDialogFilter("所有文件", "*");
/// 
/// // 使用列表形式
/// var jsonFilter = new FileDialogFilter("JSON 文件", new List&lt;string&gt; { "json" });
/// </code>
/// </example>
/// <param name="Header">过滤器的显示名称，如 "C# 源文件"。</param>
/// <param name="Extensions">文件扩展名列表（不含点号），如 ["cs", "csx"]。</param>
public record FileDialogFilter(string Header, IList<string> Extensions)
{
    /// <summary>
    /// 使用可变参数初始化 <see cref="FileDialogFilter"/> 类的新实例。
    /// </summary>
    /// <param name="header">过滤器的显示名称。</param>
    /// <param name="extensions">一个或多个文件扩展名（不含点号）。</param>
    /// <example>
    /// <code>
    /// var filter = new FileDialogFilter("图片文件", "png", "jpg", "gif");
    /// </code>
    /// </example>
    public FileDialogFilter(string header, params string[] extensions)
        : this(header, (IList<string>)extensions)
    {
    }

    /// <summary>
    /// 返回过滤器的标准字符串表示形式。
    /// </summary>
    /// <returns>
    /// 格式为 "Header|*.ext1;*.ext2" 的字符串，
    /// 与传统 Windows 文件对话框的过滤器格式兼容。
    /// </returns>
    /// <example>
    /// <code>
    /// var filter = new FileDialogFilter("C# 文件", "cs", "csx");
    /// Console.WriteLine(filter.ToString()); 
    /// // 输出: "C# 文件|*.cs;*.csx"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"{Header}|{string.Join(";", Extensions.Select(e => "*." + e))}";
}
