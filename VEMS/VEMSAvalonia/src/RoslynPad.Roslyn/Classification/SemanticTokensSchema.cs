namespace RoslynPad.Roslyn.Classification;

/// <summary>
/// 语义令牌模式映射类
/// 用于将 Roslyn 的分类类型名称映射为 LSP（语言服务器协议）的令牌名称
/// </summary>
public static class SemanticTokensSchema
{
    /// <summary>
    /// 获取标准的分类类型名称到 LSP 令牌名称的映射字典
    /// 不启用 Visual Studio 扩展相关的客户端支持
    /// </summary>
    public static IReadOnlyDictionary<string, string> ClassificationTypeNameToTokenName =>
        Microsoft.CodeAnalysis.LanguageServer.Handler.SemanticTokens.SemanticTokensSchema.GetSchema(clientSupportsVisualStudioExtensions: false).TokenTypeMap;

    /// <summary>
    /// 获取自定义的分类类型名称到 LSP 令牌名称的映射字典
    /// </summary>
    public static IReadOnlyDictionary<string, string> ClassificationTypeNameToCustomTokenName =>
        Microsoft.CodeAnalysis.LanguageServer.Handler.SemanticTokens.CustomLspSemanticTokenNames.ClassificationTypeNameToCustomTokenName;
}
