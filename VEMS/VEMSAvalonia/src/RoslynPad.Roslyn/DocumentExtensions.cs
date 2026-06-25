using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace RoslynPad.Roslyn;

/// <summary>
/// 文档扩展方法类，提供针对Document类型的通用扩展功能
/// </summary>
public static class DocumentExtensions
{
    /// <summary>
    /// 从文档中获取指定类型的语言服务
    /// </summary>
    /// <typeparam name="TLanguageService">语言服务类型，必须实现ILanguageService</typeparam>
    /// <param name="document">扩展的Document实例</param>
    /// <returns>获取到的语言服务实例</returns>
    public static TLanguageService GetLanguageService<TLanguageService>(this Document document)
        where TLanguageService : class, ILanguageService
    {
        return document.Project.Services.GetRequiredService<TLanguageService>();
    }

    /// <summary>
    /// 异步获取文档中指定位置的触达单词语法令牌
    /// </summary>
    /// <param name="document">扩展的Document实例</param>
    /// <param name="position">文本位置偏移量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="findInsideTrivia">是否在语法琐事中查找</param>
    /// <returns>触达的单词语法令牌</returns>
    public static async Task<SyntaxToken> GetTouchingWordAsync(
        this Document document,
        int position,
        CancellationToken cancellationToken,
        bool findInsideTrivia = false)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null)
        {
            return default;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var syntaxFactsService = document.GetLanguageService<ISyntaxFactsService>();
        return await syntaxTree.GetTouchingTokenAsync(
            semanticModel,
            position,
            (_, token) => syntaxFactsService.IsWord(token),
            cancellationToken,
            findInsideTrivia).ConfigureAwait(false);
    }

    /// <summary>
    /// 为文档设置冻结的部分语义
    /// </summary>
    /// <param name="document">扩展的Document实例</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>设置后的Document实例</returns>
    public static Document WithFrozenPartialSemantics(this Document document, CancellationToken cancellationToken = default)
    {
        return document.WithFrozenPartialSemantics(cancellationToken);
    }
}
