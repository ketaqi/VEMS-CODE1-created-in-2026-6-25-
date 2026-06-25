using RoslynPad.Build;

namespace RoslynPad.UI;

/// <summary>
/// 执行平台工厂接口：提供可用运行时平台的发现和创建。
/// </summary>
/// <remarks>
/// <para>
/// 此接口负责检测系统中安装的 .NET 运行时，并创建对应的执行平台实例。
/// 执行平台用于编译和运行用户脚本。
/// </para>
/// <para>
/// 支持的平台示例：
/// <list type="bullet">
///   <item><description>.NET 8.0 (Console)</description></item>
///   <item><description>.NET 9.0 (Console)</description></item>
///   <item><description>.NET Framework 4.8 (仅 Windows)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var factory = serviceProvider.GetRequiredService&lt;IPlatformsFactory&gt;();
/// 
/// // 获取所有可用平台
/// var platforms = factory.GetExecutionPlatforms();
/// foreach (var platform in platforms)
/// {
///     Console.WriteLine($"可用平台: {platform}");
/// }
/// 
/// // 获取 dotnet 可执行文件路径
/// string dotnetPath = factory.DotNetExecutable;
/// </code>
/// </example>
internal interface IPlatformsFactory
{
    /// <summary>
    /// 获取系统中所有可用的执行平台。
    /// </summary>
    /// <returns>
    /// 包含所有检测到的执行平台的只读列表。
    /// 列表按版本降序排列，最新版本在前。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 此方法会扫描系统中安装的 .NET SDK 和运行时，
    /// 返回所有可用于编译和运行脚本的平台。
    /// </para>
    /// <para>
    /// 返回的列表可能为空（如果未安装任何兼容的运行时）。
    /// </para>
    /// </remarks>
    IReadOnlyList<ExecutionPlatform> GetExecutionPlatforms();

    /// <summary>
    /// 获取 dotnet CLI 可执行文件的路径。
    /// </summary>
    /// <value>
    /// <c>dotnet</c> 或 <c>dotnet.exe</c> 的完整路径，
    /// 用于调用 CLI 命令（如 <c>dotnet build</c>）。
    /// </value>
    /// <remarks>
    /// 在 Windows 上通常为 <c>C:\Program Files\dotnet\dotnet.exe</c>，
    /// 在 Linux/macOS 上通常为 <c>/usr/share/dotnet/dotnet</c> 或 <c>/usr/local/share/dotnet/dotnet</c>。
    /// </remarks>
    string DotNetExecutable { get; }
}
