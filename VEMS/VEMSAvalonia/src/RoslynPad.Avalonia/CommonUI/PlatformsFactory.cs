using Microsoft.Win32;
using NuGet.Versioning;
using RoslynPad.Build;
using RoslynPad.UI;
using System.Composition;
using System.Runtime.InteropServices;

namespace RoslynPad;

/// <summary>
/// 执行平台工厂：检测系统中安装的 .NET 运行时并创建执行平台实例。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现 <see cref="IPlatformsFactory"/> 接口，负责发现系统中可用的 .NET 运行时：
/// <list type="bullet">
///   <item><description>.NET / .NET Core：扫描 SDK 安装目录</description></item>
///   <item><description>.NET Framework：读取 Windows 注册表（仅 Windows x64）</description></item>
/// </list>
/// </para>
/// <para>
/// SDK 搜索路径（按优先级）：
/// <list type="number">
///   <item><description><c>DOTNET_ROOT</c> 环境变量</description></item>
///   <item><description>Windows: <c>%ProgramW6432%\dotnet</c></description></item>
///   <item><description>Linux/macOS: <c>~/.dotnet</c>, <c>/usr/lib/dotnet</c>, <c>/usr/share/dotnet</c> 等</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var factory = serviceProvider.GetRequiredService&lt;IPlatformsFactory&gt;();
/// 
/// // 获取所有可用平台
/// foreach (var platform in factory.GetExecutionPlatforms())
/// {
///     Console.WriteLine($"{platform.Name} ({platform.TargetFrameworkMoniker})");
/// }
/// 
/// // 获取 dotnet CLI 路径
/// var dotnetPath = factory.DotNetExecutable;
/// </code>
/// </example>
[Export(typeof(IPlatformsFactory))]
internal class PlatformsFactory : IPlatformsFactory
{
    /// <summary>缓存的执行平台列表。</summary>
    IReadOnlyList<ExecutionPlatform>? _executionPlatforms;

    /// <summary>缓存的 dotnet 路径信息。</summary>
    private (string dotnetExe, string sdkPath) _dotnetPaths;

    /// <inheritdoc/>
    public IReadOnlyList<ExecutionPlatform> GetExecutionPlatforms() =>
        _executionPlatforms ??= GetNetVersions().Concat(GetNetFrameworkVersions()).ToArray().AsReadOnly();

    /// <inheritdoc/>
    public string DotNetExecutable => FindNetSdk().dotnetExe;

    /// <summary>
    /// 获取所有安装的 .NET / .NET Core 版本。
    /// </summary>
    /// <returns>执行平台枚举，按版本降序排列（预发布版本在后）。</returns>
    private IEnumerable<ExecutionPlatform> GetNetVersions()
    {
        var (_, sdkPath) = FindNetSdk();

        if (string.IsNullOrEmpty(sdkPath))
        {
            return [];
        }

        var versions = new List<(string name, string tfm, NuGetVersion version)>();

        foreach (var directory in IOUtilities.EnumerateDirectories(sdkPath))
        {
            var versionName = Path.GetFileName(directory);
            if (NuGetVersion.TryParse(versionName, out var version) && version.Major > 1)
            {
                var name = version.Major < 5 ? ".NET Core" : ".NET";
                var tfm = version.Major < 5 ? $"netcoreapp{version.Major}.{version.Minor}" : $"net{version.Major}.{version.Minor}";
                versions.Add((name, tfm, version));
            }
        }

        return versions.OrderBy(c => c.version.IsPrerelease).ThenByDescending(c => c.version)
            .Select(version => new ExecutionPlatform(version.name, version.tfm, version.version, Architecture.X64, isDotNet: true));
    }

    /// <summary>
    /// 获取安装的 .NET Framework 版本（仅 Windows x64）。
    /// </summary>
    /// <returns>如果检测到 .NET Framework，返回对应的执行平台；否则返回空枚举。</returns>
    private IEnumerable<ExecutionPlatform> GetNetFrameworkVersions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            var targetFrameworkName = GetNetFrameworkName();
            yield return new ExecutionPlatform(".NET Framework x64", targetFrameworkName, null, Architecture.X64, isDotNet: false);
        }
    }

    /// <summary>
    /// 查找 .NET SDK 的安装路径。
    /// </summary>
    /// <returns>包含 dotnet 可执行文件路径和 SDK 目录路径的元组。</returns>
    private (string dotnetExe, string sdkPath) FindNetSdk()
    {
        if (_dotnetPaths.dotnetExe is not null)
        {
            return _dotnetPaths;
        }

        List<string> dotnetPaths = [];
        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is var dotnetRoot && !string.IsNullOrEmpty(dotnetRoot))
        {
            dotnetPaths.Add(dotnetRoot);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            dotnetPaths.Add(Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432")!, "dotnet"));
        }
        else
        {
            dotnetPaths.AddRange([
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet"),
                "/usr/lib/dotnet",
                "/usr/lib64/dotnet",
                "/usr/share/dotnet",
                "/usr/local/share/dotnet",
            ]);
        }

        var dotnetExe = GetDotnetExe();
        var paths = (from path in dotnetPaths
                     let exePath = Path.Combine(path, dotnetExe)
                     let fullPath = Path.Combine(path, "sdk")
                     where File.Exists(exePath) && Directory.Exists(fullPath)
                     select (exePath, fullPath)).FirstOrDefault<(string exePath, string fullPath)>();

        if (paths.exePath is null)
        {
            paths = (string.Empty, string.Empty);
        }

        _dotnetPaths = paths;
        return paths;
    }

    /// <summary>
    /// 获取 dotnet 可执行文件名称。
    /// </summary>
    /// <returns>Windows 返回 "dotnet.exe"；其他平台返回 "dotnet"。</returns>
    private static string GetDotnetExe() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";

    /// <summary>
    /// 从注册表获取 .NET Framework 的目标框架名称。
    /// </summary>
    /// <returns>目标框架名称（如 "net48"）；如果未找到返回空字符串。</returns>
    private static string GetNetFrameworkName()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return string.Empty;
        }

        const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

        using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
        {
            var release = ndpKey?.GetValue("Release") as int?;
            if (release != null)
            {
                return GetNetFrameworkTargetName(release.Value);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 将 .NET Framework 发布版本号转换为目标框架名称。
    /// </summary>
    /// <param name="releaseKey">注册表中的 Release 值。</param>
    /// <returns>对应的目标框架名称。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当发布版本号低于 .NET 4.6 时抛出。</exception>
    private static string GetNetFrameworkTargetName(int releaseKey) => releaseKey switch
    {
        >= 528040 => "net48",
        >= 461808 => "net472",
        >= 461308 => "net471",
        >= 460798 => "net47",
        >= 394802 => "net462",
        >= 394254 => "net461",
        >= 393295 => "net46",
        _ => throw new ArgumentOutOfRangeException(nameof(releaseKey))
    };
}
