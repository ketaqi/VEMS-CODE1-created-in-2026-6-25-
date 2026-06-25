using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace RoslynPad.Roslyn.CodeFixes;

/// <summary>
/// 代码修复集合的封装类，包装底层Roslyn的CodeFixCollection对象
/// </summary>
public sealed class CodeFixCollection
{
    private readonly Microsoft.CodeAnalysis.CodeFixes.CodeFixCollection _inner;

    /// <summary>
    /// 初始化代码修复集合实例
    /// </summary>
    /// <param name="inner">底层Roslyn原生的CodeFixCollection实例</param>
    internal CodeFixCollection(Microsoft.CodeAnalysis.CodeFixes.CodeFixCollection inner)
    {
        _inner = inner;
        Fixes = inner.Fixes.Select(x => new CodeFix(x)).ToImmutableArray();
    }

    /// <summary>
    /// 获取代码修复提供程序
    /// </summary>
    public object Provider => _inner.Provider;

    /// <summary>
    /// 获取代码修复对应的文本范围
    /// </summary>
    public TextSpan TextSpan => _inner.TextSpan;

    /// <summary>
    /// 获取代码修复项的不可变数组
    /// </summary>
    public ImmutableArray<CodeFix> Fixes { get; }
}
