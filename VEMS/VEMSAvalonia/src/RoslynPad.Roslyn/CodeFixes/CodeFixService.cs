using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace RoslynPad.Roslyn.CodeFixes;

/// <summary>
/// 代码修复服务的实现类，基于MEF导出，包装底层Roslyn的ICodeFixService
/// </summary>
/// <param name="inner">底层Roslyn原生的ICodeFixService实例</param>
[Export(typeof(ICodeFixService)), Shared]
[method: ImportingConstructor]
internal sealed class CodeFixService(Microsoft.CodeAnalysis.CodeFixes.ICodeFixService inner) : ICodeFixService
{
    /// <summary>
    /// 异步流式获取指定文档和文本范围的代码修复集合，转换为自定义的CodeFixCollection类型
    /// </summary>
    /// <param name="document">需要修复的代码文档</param>
    /// <param name="textSpan">需要修复的文本范围</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>自定义CodeFixCollection的异步可枚举对象</returns>
    public IAsyncEnumerable<CodeFixCollection> StreamFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
    {
        var result = inner.StreamFixesAsync(document, textSpan, cancellationToken);
        return result.Select(x => new CodeFixCollection(x));
    }

    /// <summary>
    /// 转发调用底层服务，获取指定语言和诊断ID对应的抑制修复器
    /// </summary>
    /// <param name="language">目标编程语言（如C#、VB等）</param>
    /// <param name="diagnosticIds">需要抑制的诊断ID集合</param>
    /// <returns>匹配的代码修复提供程序，无匹配则返回null</returns>
    public CodeFixProvider? GetSuppressionFixer(string language, IEnumerable<string> diagnosticIds)
    {
        return inner.GetSuppressionFixer(language, diagnosticIds);
    }
}
