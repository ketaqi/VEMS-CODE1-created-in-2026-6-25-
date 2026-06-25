using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Collections.Generic;

namespace RoslynPad.Roslyn.CodeFixes;

/// <summary>
/// 代码修复服务的核心接口，定义代码修复相关的核心操作
/// </summary>
public interface ICodeFixService
{
    /// <summary>
    /// 异步流式获取指定文档和文本范围的代码修复集合
    /// </summary>
    /// <param name="document">需要修复的代码文档</param>
    /// <param name="textSpan">需要修复的文本范围</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>代码修复集合的异步可枚举对象</returns>
    IAsyncEnumerable<CodeFixCollection> StreamFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);

    /// <summary>
    /// 获取指定语言和诊断ID对应的抑制修复器
    /// </summary>
    /// <param name="language">目标编程语言（如C#、VB等）</param>
    /// <param name="diagnosticIds">需要抑制的诊断ID集合</param>
    /// <returns>匹配的代码修复提供程序，无匹配则返回null</returns>
    CodeFixProvider? GetSuppressionFixer(string language, IEnumerable<string> diagnosticIds);
}
