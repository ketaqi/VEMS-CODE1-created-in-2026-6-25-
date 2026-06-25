using System.Runtime.InteropServices;
using NuGet.Versioning;

namespace RoslynPad.Build;

/// <summary>
/// 表示代码执行的目标平台配置。
/// </summary>
public class ExecutionPlatform
{
    /// <summary>
    /// 获取平台名称。
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// 获取目标框架名称（如 "net8.0"）。
    /// </summary>
    internal string TargetFrameworkMoniker { get; }

    /// <summary>
    /// 获取框架版本。
    /// </summary>
    internal NuGetVersion? FrameworkVersion { get; }

    /// <summary>
    /// 获取目标 CPU 架构。
    /// </summary>
    internal Architecture Architecture { get; }

    /// <summary>
    /// 获取是否为 .NET（Core）平台。
    /// </summary>
    internal bool IsDotNet { get; }

    /// <summary>
    /// 获取平台描述字符串。
    /// </summary>
    internal string Description { get; }

    /// <summary>
    /// 获取是否为 .NET Framework 平台。
    /// </summary>
    internal bool IsDotNetFramework => !IsDotNet;

    /// <summary>
    /// 初始化 <see cref="ExecutionPlatform"/> 类的新实例。
    /// </summary>
    /// <param name="name">平台名称。</param>
    /// <param name="targetFrameworkMoniker">目标框架名称。</param>
    /// <param name="frameworkVersion">框架版本。</param>
    /// <param name="architecture">CPU 架构。</param>
    /// <param name="isDotNet">是否为 .NET（Core）平台。</param>
    internal ExecutionPlatform(string name, string targetFrameworkMoniker, NuGetVersion? frameworkVersion, Architecture architecture, bool isDotNet)
    {
        Name = name;
        TargetFrameworkMoniker = targetFrameworkMoniker;
        FrameworkVersion = frameworkVersion;
        Architecture = architecture;
        IsDotNet = isDotNet;
        Description = $"{Name} {FrameworkVersion}";
    }

    /// <summary>
    /// 返回平台的描述字符串。
    /// </summary>
    public override string ToString() => Description;
}
