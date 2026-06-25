using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Build;

/// <summary>
/// 执行宿主的配置参数。
/// </summary>
/// <param name="buildPath">构建输出路径。</param>
/// <param name="nuGetConfigPath">NuGet 配置文件路径。</param>
/// <param name="imports">全局 using 命名空间列表。</param>
/// <param name="disabledDiagnostics">禁用的诊断 ID 集合。</param>
/// <param name="workingDirectory">工作目录。</param>
/// <param name="sourceCodeKind">源代码类型（脚本或常规）。</param>
/// <param name="checkOverflow">是否启用溢出检查。</param>
/// <param name="allowUnsafe">是否允许不安全代码。</param>
internal class ExecutionHostParameters(
    string buildPath,
    string nuGetConfigPath,
    ImmutableArray<string> imports,
    ImmutableHashSet<string> disabledDiagnostics,
    string workingDirectory,
    SourceCodeKind sourceCodeKind,
    bool checkOverflow = false,
    bool allowUnsafe = true)
{
    /// <summary>
    /// 获取构建输出路径。
    /// </summary>
    public string BuildPath { get; } = buildPath;

    /// <summary>
    /// 获取 NuGet 配置文件路径。
    /// </summary>
    public string NuGetConfigPath { get; } = nuGetConfigPath;

    /// <summary>
    /// 获取或设置全局 using 命名空间列表。
    /// </summary>
    public ImmutableArray<string> Imports { get; set; } = imports;

    /// <summary>
    /// 获取禁用的诊断 ID 集合。
    /// </summary>
    public ImmutableHashSet<string> DisabledDiagnostics { get; } = disabledDiagnostics;

    /// <summary>
    /// 获取或设置工作目录。
    /// </summary>
    public string WorkingDirectory { get; set; } = workingDirectory;

    /// <summary>
    /// 获取或设置源代码类型。
    /// </summary>
    public SourceCodeKind SourceCodeKind { get; set; } = sourceCodeKind;

    /// <summary>
    /// 获取是否启用溢出检查。
    /// </summary>
    public bool CheckOverflow { get; } = checkOverflow;

    /// <summary>
    /// 获取是否允许不安全代码。
    /// </summary>
    public bool AllowUnsafe { get; } = allowUnsafe;
}
