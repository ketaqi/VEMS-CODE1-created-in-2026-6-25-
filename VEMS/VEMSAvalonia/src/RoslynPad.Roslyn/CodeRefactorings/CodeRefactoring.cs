using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace RoslynPad.Roslyn.CodeRefactorings;

/// <summary>
/// 代码重构封装类，用于包装Roslyn原生的代码重构对象
/// </summary>
public sealed class CodeRefactoring
{
    /// <summary>
    /// 获取代码重构提供程序
    /// </summary>
    public CodeRefactoringProvider Provider { get; }

    /// <summary>
    /// 获取代码重构对应的操作集合
    /// </summary>
    public ImmutableArray<CodeAction> Actions { get; }

    /// <summary>
    /// 初始化 <see cref="CodeRefactoring"/> 类的新实例
    /// </summary>
    /// <param name="inner">Roslyn原生的代码重构对象</param>
    internal CodeRefactoring(Microsoft.CodeAnalysis.CodeRefactorings.CodeRefactoring inner)
    {
        Provider = inner.Provider;
        Actions = inner.CodeActions.Select(c => c.action).ToImmutableArray();
    }
}
