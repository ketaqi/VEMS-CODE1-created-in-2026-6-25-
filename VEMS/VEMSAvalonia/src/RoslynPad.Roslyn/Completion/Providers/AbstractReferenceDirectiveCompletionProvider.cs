using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.PooledObjects;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 引用指令（#r）路径补全提供程序的抽象基类，处理程序集相关的路径补全
/// </summary>
internal abstract class AbstractReferenceDirectiveCompletionProvider : AbstractDirectivePathCompletionProvider
{
    /// <summary>
    /// 引用指令补全项的规则配置
    /// </summary>
    private static readonly CompletionItemRules s_rules = CompletionItemRules.Create(
        filterCharacterRules: [],
        commitCharacterRules: [CharacterSetModificationRule.Create(CharacterSetModificationKind.Replace, GetCommitCharacters())],
        enterKeyRule: EnterKeyRule.Never,
        selectionBehavior: CompletionItemSelectionBehavior.HardSelection);

    /// <summary>
    /// 路径标识字符（用于判断是否为文件路径，而非GAC程序集名称）
    /// </summary>
    private static readonly char[] s_pathIndicators = ['/', '\\', ':'];

    /// <summary>
    /// 获取引用指令补全的提交字符集合
    /// </summary>
    /// <returns>不可变的提交字符数组</returns>
    private static ImmutableArray<char> GetCommitCharacters()
    {
        var builder = ArrayBuilder<char>.GetInstance();

        builder.Add('"'); // 引号作为提交字符

        if (PathUtilities.IsUnixLikePlatform)
        {
            builder.Add('/'); // Unix/Linux 下的路径分隔符
        }
        else
        {
            builder.Add('/'); // Windows 下的正斜杠
            builder.Add('\\'); // Windows 下的反斜杠
        }

        builder.Add(','); // 程序集名称分隔符

        return builder.ToImmutableAndFree();
    }

    /// <summary>
    /// 提供引用指令的补全项（GAC程序集 + 文件系统程序集）
    /// </summary>
    /// <param name="context">补全上下文</param>
    /// <param name="pathThroughLastSlash">最后一个路径分隔符（含）之前的路径</param>
    /// <returns>异步任务</returns>
    protected override async Task ProvideCompletionsAsync(CompletionContext context, string pathThroughLastSlash)
    {
        // 若路径无路径标识字符，尝试从GAC获取程序集补全
        if (GacFileResolver.IsAvailable && pathThroughLastSlash.IndexOfAny(s_pathIndicators) < 0)
        {
            var gacHelper = new GlobalAssemblyCacheCompletionHelper(s_rules);
            context.AddItems(await gacHelper.GetItemsAsync(pathThroughLastSlash, context.CancellationToken).ConfigureAwait(false));
        }

        // 若路径无逗号（排除程序集版本分隔），从文件系统获取dll/exe补全
        if (pathThroughLastSlash.IndexOf(',') < 0)
        {
            var helper = GetFileSystemCompletionHelper(context.Document, Microsoft.CodeAnalysis.Glyph.Assembly, [".dll", ".exe"], s_rules);
            context.AddItems(await helper.GetItemsAsync(pathThroughLastSlash, context.CancellationToken).ConfigureAwait(false));
        }
    }
}
