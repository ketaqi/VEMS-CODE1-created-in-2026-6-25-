using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.ExternalAccess.Pythia.Api;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// Pythia签名帮助提供程序实现类，适配Pythia外部访问接口
/// </summary>
[Export(typeof(IPythiaSignatureHelpProviderImplementation))]
internal class PythiaSignatureHelpProviderImplementation : IPythiaSignatureHelpProviderImplementation
{
    /// <summary>
    /// 异步获取方法组的签名帮助项及选中项索引
    /// </summary>
    /// <param name="accessibleMethods">可访问的方法符号集合</param>
    /// <param name="document">当前文档</param>
    /// <param name="invocationExpression">调用表达式语法节点</param>
    /// <param name="semanticModel">语义模型</param>
    /// <param name="currentSymbol">当前符号信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含签名帮助项集合和选中项索引的元组（当前返回空集合和null）</returns>
    public Task<(ImmutableArray<PythiaSignatureHelpItemWrapper> items, int? selectedItemIndex)> GetMethodGroupItemsAndSelectionAsync(
        ImmutableArray<IMethodSymbol> accessibleMethods,
        Document document,
        InvocationExpressionSyntax invocationExpression,
        SemanticModel semanticModel,
        SymbolInfo currentSymbol,
        CancellationToken cancellationToken)
    {
        return Task.FromResult((ImmutableArray<PythiaSignatureHelpItemWrapper>.Empty, (int?)null));
    }
}
