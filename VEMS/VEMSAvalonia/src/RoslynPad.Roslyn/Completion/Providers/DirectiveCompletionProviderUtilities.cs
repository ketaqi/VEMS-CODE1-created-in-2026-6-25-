using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 指令补全提供程序的工具类，提供通用的语法树操作方法
/// </summary>
internal static class DirectiveCompletionProviderUtilities
{
    /// <summary>
    /// 从语法树指定位置获取指定类型指令的字符串字面量令牌
    /// </summary>
    /// <param name="tree">语法树实例</param>
    /// <param name="position">代码中的位置偏移量</param>
    /// <param name="directiveKind">指令的语法类型</param>
    /// <param name="stringLiteral">输出参数，匹配到的字符串字面量令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功获取指定指令的字符串字面量令牌</returns>
    internal static bool TryGetStringLiteralToken(this SyntaxTree tree, int position, SyntaxKind directiveKind, out SyntaxToken stringLiteral, CancellationToken cancellationToken)
    {
        // 检查位置是否在字符串字面量范围内
        if (tree.IsEntirelyWithinStringLiteral(position, cancellationToken))
        {
            var token = tree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);

            // 处理指令结束/文件结束令牌，回退到前一个令牌
            if (token.IsKind(SyntaxKind.EndOfDirectiveToken) || token.IsKind(SyntaxKind.EndOfFileToken))
            {
                token = token.GetPreviousToken(includeSkipped: true, includeDirectives: true);
            }

            // 验证令牌类型及所属父节点类型
            if (token.IsKind(SyntaxKind.StringLiteralToken) && token.Parent?.IsKind(directiveKind) is true)
            {
                stringLiteral = token;
                return true;
            }
        }

        stringLiteral = default;
        return false;
    }
}
