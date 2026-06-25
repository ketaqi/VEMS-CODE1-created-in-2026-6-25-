namespace RoslynPad.UI;

/// <summary>
/// 重命名符号对话框接口：用于重命名代码中的标识符（变量、方法、类等）。
/// </summary>
/// <remarks>
/// <para>
/// 此对话框配合 Roslyn 的重命名功能使用，允许用户为代码中的符号指定新名称。
/// 重命名操作将自动更新所有引用该符号的位置。
/// </para>
/// <para>
/// 典型使用流程：
/// <list type="number">
///   <item><description>用户选中要重命名的符号</description></item>
///   <item><description>调用 <see cref="Initialize"/> 方法设置当前符号名称</description></item>
///   <item><description>显示对话框让用户输入新名称</description></item>
///   <item><description>检查 <see cref="ShouldRename"/> 确认用户是否确认重命名</description></item>
///   <item><description>使用 <see cref="SymbolName"/> 获取新名称并执行重命名</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;IRenameSymbolDialog&gt;();
/// dialog.Initialize("oldVariableName");
/// 
/// await dialog.ShowAsync();
/// 
/// if (dialog.ShouldRename &amp;&amp; !string.IsNullOrEmpty(dialog.SymbolName))
/// {
///     await roslynHost.RenameSymbolAsync(symbol, dialog.SymbolName);
/// }
/// </code>
/// </example>
public interface IRenameSymbolDialog : IDialog
{
    /// <summary>
    /// 获取用户是否确认执行重命名操作。
    /// </summary>
    /// <value>
    /// <c>true</c> 如果用户点击了"重命名"或"确定"按钮；
    /// <c>false</c> 如果用户取消了操作。
    /// </value>
    /// <remarks>
    /// 调用方应在 <see cref="IDialog.ShowAsync"/> 返回后检查此属性，
    /// 以确定是否执行实际的重命名操作。
    /// </remarks>
    bool ShouldRename { get; }

    /// <summary>
    /// 获取或设置符号的新名称。
    /// </summary>
    /// <value>
    /// 用户输入的新符号名称。如果用户未修改，则为初始化时设置的原名称。
    /// </value>
    /// <remarks>
    /// 此属性在调用 <see cref="Initialize"/> 后会被设置为原符号名称，
    /// 用户可在对话框中修改此值。
    /// </remarks>
    string? SymbolName { get; set; }

    /// <summary>
    /// 使用指定的符号名称初始化对话框。
    /// </summary>
    /// <param name="symbolName">要重命名的当前符号名称。</param>
    /// <remarks>
    /// <para>
    /// 此方法应在调用 <see cref="IDialog.ShowAsync"/> 之前调用，
    /// 用于设置对话框中输入框的初始值。
    /// </para>
    /// <para>
    /// 调用此方法后，<see cref="SymbolName"/> 将被设置为 <paramref name="symbolName"/>。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dialog = serviceProvider.GetRequiredService&lt;IRenameSymbolDialog&gt;();
    /// 
    /// // 初始化为当前符号名称
    /// dialog.Initialize("myVariable");
    /// 
    /// // 显示对话框
    /// await dialog.ShowAsync();
    /// 
    /// // 获取用户输入的新名称
    /// if (dialog.ShouldRename)
    /// {
    ///     string newName = dialog.SymbolName; // 例如 "myNewVariable"
    /// }
    /// </code>
    /// </example>
    void Initialize(string symbolName);
}
