// C# 语言括号匹配抽象类，基于 SyntaxKind 初始化括号信息

using Microsoft.CodeAnalysis.CSharp;

namespace RoslynPad.Roslyn.BraceMatching;

internal abstract class AbstractCSharpBraceMatcher : AbstractBraceMatcher
{
    /// <summary>
    /// 通过 SyntaxKind 传入 C# 括号类型
    /// </summary>
    protected AbstractCSharpBraceMatcher(SyntaxKind openBrace, SyntaxKind closeBrace)
        : base(new BraceCharacterAndKind(SyntaxFacts.GetText(openBrace)[0], (int)openBrace),
               new BraceCharacterAndKind(SyntaxFacts.GetText(closeBrace)[0], (int)closeBrace))
    {
    }
}
