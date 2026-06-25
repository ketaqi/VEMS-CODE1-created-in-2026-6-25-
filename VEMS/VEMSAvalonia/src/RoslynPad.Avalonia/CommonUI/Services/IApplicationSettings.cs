namespace RoslynPad.UI;

/// <summary>
/// 应用程序设置接口：定义应用配置的加载和访问契约。
/// </summary>
/// <remarks>
/// <para>
/// 此接口提供应用程序配置的加载机制和访问入口。
/// 具体实现负责从配置文件（如 JSON、XML）读取设置，
/// 并通过 <see cref="Values"/> 属性暴露可读写的配置值。
/// </para>
/// <para>
/// 职责分离：
/// <list type="bullet">
///   <item><description><see cref="IApplicationSettings"/> - 负责加载/保存配置</description></item>
///   <item><description><see cref="IApplicationSettingsValues"/> - 负责存储具体配置值</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 加载默认配置
/// var settings = serviceProvider.GetRequiredService&lt;IApplicationSettings&gt;();
/// settings.LoadDefault();
/// 
/// // 访问配置值
/// int fontSize = settings.Values.EditorFontSize;
/// string docPath = settings.Values.DocumentPath;
/// </code>
/// </example>
public interface IApplicationSettings
{
    /// <summary>
    /// 从默认位置加载应用程序设置。
    /// </summary>
    /// <remarks>
    /// 默认位置通常为用户配置目录下的设置文件，
    /// 如 <c>%AppData%/RoslynPad/settings.json</c>。
    /// </remarks>
    void LoadDefault();

    /// <summary>
    /// 从指定路径加载应用程序设置。
    /// </summary>
    /// <param name="path">配置文件的完整路径。</param>
    /// <exception cref="System.IO.FileNotFoundException">当指定的配置文件不存在时抛出。</exception>
    /// <exception cref="System.Text.Json.JsonException">当配置文件格式无效时抛出。</exception>
    void LoadFrom(string path);

    /// <summary>
    /// 获取默认的文档存储路径。
    /// </summary>
    /// <returns>
    /// 用户文档的默认存储目录路径，通常为 <c>~/Documents/RoslynPad</c> 或类似位置。
    /// </returns>
    /// <remarks>
    /// 此路径用于存储用户创建的脚本文件和项目。
    /// 如果目录不存在，实现应负责创建。
    /// </remarks>
    string GetDefaultDocumentPath();

    /// <summary>
    /// 获取应用程序设置值的访问器。
    /// </summary>
    /// <value>
    /// 包含所有可配置项的设置值对象。
    /// </value>
    /// <seealso cref="IApplicationSettingsValues"/>
    IApplicationSettingsValues Values { get; }
}
