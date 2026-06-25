namespace RoslynPad.UI;

/// <summary>
/// 保存文档对话框接口：用于在关闭或切换文档时提示用户保存更改。
/// </summary>
/// <remarks>
/// <para>
/// 此对话框通常在以下场景使用：
/// <list type="bullet">
///   <item><description>关闭已修改但未保存的文档</description></item>
///   <item><description>退出应用程序时存在未保存的更改</description></item>
///   <item><description>切换到其他文档前保存当前更改</description></item>
/// </list>
/// </para>
/// <para>
/// 对话框提供三种选择：保存、不保存、取消操作。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;ISaveDocumentDialog&gt;();
/// dialog.DocumentName = "MyScript.csx";
/// dialog.AllowNameEdit = true;
/// dialog.ShowDoNotSave = true;
/// dialog.FilePathFactory = name => Path.Combine(documentsFolder, name);
/// 
/// await dialog.ShowAsync();
/// 
/// switch (dialog.Result)
/// {
///     case SaveResult.Save:
///         await SaveDocument(dialog.FilePath);
///         break;
///     case SaveResult.DoNotSave:
///         // 放弃更改，继续操作
///         break;
///     case SaveResult.Cancel:
///         // 用户取消，中止操作
///         return;
/// }
/// </code>
/// </example>
public interface ISaveDocumentDialog : IDialog
{
    /// <summary>
    /// 获取或设置文档名称。
    /// </summary>
    /// <value>
    /// 要保存的文档名称（通常为文件名，不含路径）。
    /// 当 <see cref="AllowNameEdit"/> 为 <c>true</c> 时，用户可修改此值。
    /// </value>
    string? DocumentName { get; set; }

    /// <summary>
    /// 获取用户的操作结果。
    /// </summary>
    /// <value>
    /// 对话框关闭后的用户选择结果。
    /// </value>
    /// <seealso cref="SaveResult"/>
    SaveResult Result { get; }

    /// <summary>
    /// 获取或设置是否允许用户编辑文档名称。
    /// </summary>
    /// <value>
    /// <c>true</c> 显示可编辑的文件名输入框；
    /// <c>false</c> 仅显示只读文件名（默认）。
    /// </value>
    /// <remarks>
    /// 当设置为 <c>true</c> 时，用户可以在保存前修改文件名，
    /// 适用于"另存为"场景。
    /// </remarks>
    bool AllowNameEdit { get; set; }

    /// <summary>
    /// 获取或设置是否显示"不保存"按钮。
    /// </summary>
    /// <value>
    /// <c>true</c> 显示"不保存"按钮，允许用户放弃更改；
    /// <c>false</c> 仅显示"保存"和"取消"按钮。
    /// </value>
    /// <remarks>
    /// 在某些场景（如退出应用程序）下，允许用户选择不保存更改直接退出。
    /// </remarks>
    bool ShowDoNotSave { get; set; }

    /// <summary>
    /// 获取最终确定的文件保存路径。
    /// </summary>
    /// <value>
    /// 由 <see cref="FilePathFactory"/> 根据 <see cref="DocumentName"/> 生成的完整文件路径；
    /// 如果未设置工厂函数，可能为 <c>null</c>。
    /// </value>
    string? FilePath { get; }

    /// <summary>
    /// 获取或设置文件路径生成工厂。
    /// </summary>
    /// <value>
    /// 一个函数，接受文档名称作为参数，返回完整的文件保存路径。
    /// </value>
    /// <example>
    /// <code>
    /// dialog.FilePathFactory = name => Path.Combine(
    ///     Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    ///     "RoslynPad",
    ///     name
    /// );
    /// </code>
    /// </example>
    Func<string, string>? FilePathFactory { get; set; }
}

/// <summary>
/// 保存对话框的用户操作结果。
/// </summary>
/// <remarks>
/// 此枚举表示用户在保存文档对话框中的选择。
/// </remarks>
public enum SaveResult
{
    /// <summary>
    /// 用户取消了操作（点击"取消"按钮或关闭对话框）。
    /// </summary>
    /// <remarks>
    /// 当返回此值时，调用方应中止当前操作（如关闭文档或退出应用）。
    /// </remarks>
    Cancel,

    /// <summary>
    /// 用户选择保存文档（点击"保存"按钮）。
    /// </summary>
    /// <remarks>
    /// 当返回此值时，调用方应使用 <see cref="ISaveDocumentDialog.FilePath"/> 保存文档。
    /// </remarks>
    Save,

    /// <summary>
    /// 用户选择不保存文档（点击"不保存"按钮）。
    /// </summary>
    /// <remarks>
    /// 当返回此值时，调用方应放弃未保存的更改并继续原定操作。
    /// 此选项仅在 <see cref="ISaveDocumentDialog.ShowDoNotSave"/> 为 <c>true</c> 时可用。
    /// </remarks>
    DoNotSave
}
