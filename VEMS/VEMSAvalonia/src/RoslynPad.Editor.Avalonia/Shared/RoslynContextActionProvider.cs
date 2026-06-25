using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.CodeActions;
using RoslynPad.Roslyn.CodeFixes;
using RoslynPad.Roslyn.CodeRefactorings;

namespace RoslynPad.Editor;

/// <summary>
/// 基于Roslyn的上下文动作提供程序，提供代码修复和代码重构功能
/// </summary>
public sealed class RoslynContextActionProvider : IContextActionProvider
{
    /// <summary>
    /// 排除的重构提供程序列表（避免某些不需要的重构选项）
    /// </summary>
    private static readonly ImmutableArray<string> s_excludedRefactoringProviders =
        ["ExtractInterface"];

    /// <summary>
    /// 当前文档的唯一标识
    /// </summary>
    private readonly DocumentId _documentId;

    /// <summary>
    /// Roslyn宿主服务
    /// </summary>
    private readonly IRoslynHost _roslynHost;

    /// <summary>
    /// 代码修复服务，用于获取代码错误/警告的修复建议
    /// </summary>
    private readonly ICodeFixService _codeFixService;

    /// <summary>
    /// 初始化 <see cref="RoslynContextActionProvider"/> 实例
    /// </summary>
    /// <param name="documentId">当前文档ID</param>
    /// <param name="roslynHost">Roslyn宿主服务实例</param>
    public RoslynContextActionProvider(DocumentId documentId, IRoslynHost roslynHost)
    {
        _documentId = documentId;
        _roslynHost = roslynHost;
        _codeFixService = _roslynHost.GetService<ICodeFixService>();
    }

    /// <summary>
    /// 获取指定文本范围的上下文动作（代码修复+代码重构）
    /// </summary>
    /// <param name="offset">文本起始偏移量</param>
    /// <param name="length">文本长度</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上下文动作列表</returns>
    public async Task<IEnumerable<object>> GetActions(int offset, int length, CancellationToken cancellationToken)
    {
        var textSpan = new TextSpan(offset, length);
        var document = _roslynHost.GetDocument(_documentId);
        if (document == null)
        {
            return [];
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        if (textSpan.End >= text.Length) return [];

        // 获取代码修复建议
        var codeFixes = await _codeFixService.StreamFixesAsync(document, textSpan, cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // 获取代码重构建议
        var codeRefactorings = await _roslynHost.GetService<ICodeRefactoringService>().GetRefactoringsAsync(
            document,
            textSpan, cancellationToken).ConfigureAwait(false);

        // 合并代码修复和重构动作（排除指定的重构提供程序）
        return ((IEnumerable<object>)codeFixes.SelectMany(x => x.Fixes))
            .Concat(codeRefactorings
                .Where(x => s_excludedRefactoringProviders.All(p => !x.Provider.GetType().Name.Contains(p)))
                .SelectMany(x => x.Actions));
    }

    /// <summary>
    /// 获取上下文动作对应的执行命令
    /// </summary>
    /// <param name="action">上下文动作实例</param>
    /// <returns>可执行的命令，无对应命令则返回null</returns>
    public ICommand? GetActionCommand(object action)
    {
        if (action is CodeAction codeAction)
        {
            return new CodeActionCommand(this, codeAction);
        }

        if (action is not CodeFix codeFix || codeFix.Action.HasCodeActions()) return null;
        return new CodeActionCommand(this, codeFix.Action);
    }

    /// <summary>
    /// 执行Roslyn代码动作
    /// </summary>
    /// <param name="codeAction">代码动作实例</param>
    /// <returns>异步任务</returns>
    private async Task ExecuteCodeAction(CodeAction codeAction)
    {
        var operations = await codeAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(true);
        foreach (var operation in operations)
        {
            var document = _roslynHost.GetDocument(_documentId);
            if (document != null)
            {
                // 将代码动作应用到工作区
                operation.Apply(document.Project.Solution.Workspace,
                    CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// 封装代码动作的执行命令
    /// </summary>
    private class CodeActionCommand(RoslynContextActionProvider provider, CodeAction codeAction) : ICommand
    {
        private readonly RoslynContextActionProvider _provider = provider;
        private readonly CodeAction _codeAction = codeAction;

        /// <summary>
        /// 命令可执行状态变更事件（空实现）
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// 检查命令是否可执行
        /// </summary>
        /// <param name="parameter">命令参数（未使用）</param>
        /// <returns>始终返回true</returns>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// 执行代码动作命令
        /// </summary>
        /// <param name="parameter">命令参数（未使用）</param>
        public async void Execute(object? parameter)
        {
            await _provider.ExecuteCodeAction(_codeAction).ConfigureAwait(true);
        }
    }
}
