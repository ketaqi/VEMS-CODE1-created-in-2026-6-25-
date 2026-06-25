using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.CodeRefactorings;

/// <summary>
/// 代码重构服务实现类，封装Roslyn原生的代码重构服务，提供统一的重构操作入口
/// </summary>
[Export(typeof(ICodeRefactoringService)), Shared]
[method: ImportingConstructor]
internal sealed class CodeRefactoringService(Microsoft.CodeAnalysis.CodeRefactorings.ICodeRefactoringService inner) : ICodeRefactoringService
{
    /// <summary>
    /// 异步检查指定文档和文本范围是否存在可用的代码重构
    /// </summary>
    /// <param name="document">要检查的文档</param>
    /// <param name="textSpan">要检查的文本范围</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存在可用重构返回true，否则返回false</returns>
    public Task<bool> HasRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
    {
        return inner.HasRefactoringsAsync(document, textSpan, cancellationToken);
    }

    /// <summary>
    /// 异步获取指定文档和文本范围的代码重构列表
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="textSpan">目标文本范围</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>代码重构对象集合</returns>
    public async Task<IEnumerable<CodeRefactoring>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
    {
        var result = await inner.GetRefactoringsAsync(document, textSpan, CodeActionRequestPriority.Default, cancellationToken).ConfigureAwait(false);
        return result.Select(x => new CodeRefactoring(x)).ToArray();
    }
}
