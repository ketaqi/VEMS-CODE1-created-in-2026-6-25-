using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace RoslynPad.Roslyn.CodeFixes;

/// <summary>
/// 代码修复项的封装类，包装底层Roslyn的CodeFix对象
/// </summary>
/// <param name="inner">底层Roslyn原生的CodeFix实例</param>
public sealed class CodeFix(Microsoft.CodeAnalysis.CodeFixes.CodeFix inner)
{
    /// <summary>
    /// 获取代码修复所属的项目
    /// </summary>
    public Project Project => inner.Project;

    /// <summary>
    /// 获取代码修复对应的操作行为
    /// </summary>
    public CodeAction Action => inner.Action;

    /// <summary>
    /// 获取代码修复关联的诊断信息集合
    /// </summary>
    public ImmutableArray<Diagnostic> Diagnostics => inner.Diagnostics;

    /// <summary>
    /// 获取代码修复的主诊断信息
    /// </summary>
    public Diagnostic PrimaryDiagnostic => inner.PrimaryDiagnostic;
}
