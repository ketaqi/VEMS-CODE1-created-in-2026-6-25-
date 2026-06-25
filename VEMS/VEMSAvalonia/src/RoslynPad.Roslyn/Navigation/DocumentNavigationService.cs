using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynPad.Roslyn.Navigation;

/// <summary>
/// 文档导航服务实现类，用于处理Roslyn工作区中的文档导航相关操作
/// 实现了<see cref="IDocumentNavigationService"/>接口，提供导航能力的基础支持
/// </summary>
[ExportWorkspaceService(typeof(IDocumentNavigationService))]
internal sealed class DocumentNavigationService : IDocumentNavigationService
{
    /// <summary>
    /// 异步判断是否可以导航到指定文档的指定文本范围
    /// </summary>
    /// <param name="workspace">所属的工作区实例</param>
    /// <param name="documentId">目标文档的唯一标识</param>
    /// <param name="textSpan">要导航到的文本范围</param>
    /// <param name="allowInvalidSpan">是否允许无效的文本范围</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>始终返回True，表示支持导航到指定范围</returns>
    public Task<bool> CanNavigateToSpanAsync(Workspace workspace, DocumentId documentId, TextSpan textSpan, bool allowInvalidSpan, CancellationToken cancellationToken)
        => Task.FromResult(true);

    /// <summary>
    /// 异步判断是否可以导航到指定文档的指定位置
    /// </summary>
    /// <param name="workspace">所属的工作区实例</param>
    /// <param name="documentId">目标文档的唯一标识</param>
    /// <param name="position">要导航到的字符位置</param>
    /// <param name="virtualSpace">虚拟空间偏移量</param>
    /// <param name="allowInvalidPosition">是否允许无效的位置</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>始终返回True，表示支持导航到指定位置</returns>
    public Task<bool> CanNavigateToPositionAsync(Workspace workspace, DocumentId documentId, int position, int virtualSpace, bool allowInvalidPosition, CancellationToken cancellationToken)
        => Task.FromResult(true);

    /// <summary>
    /// 异步获取指定文档文本范围对应的可导航位置
    /// </summary>
    /// <param name="workspace">所属的工作区实例</param>
    /// <param name="documentId">目标文档的唯一标识</param>
    /// <param name="textSpan">要导航到的文本范围</param>
    /// <param name="allowInvalidSpan">是否允许无效的文本范围</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>始终返回Null，表示暂未实现具体的位置解析逻辑</returns>
    public Task<INavigableLocation?> GetLocationForSpanAsync(Workspace workspace, DocumentId documentId, TextSpan textSpan, bool allowInvalidSpan, CancellationToken cancellationToken)
        => Task.FromResult<INavigableLocation?>(null);

    /// <summary>
    /// 异步获取指定文档位置对应的可导航位置
    /// </summary>
    /// <param name="workspace">所属的工作区实例</param>
    /// <param name="documentId">目标文档的唯一标识</param>
    /// <param name="position">要导航到的字符位置</param>
    /// <param name="virtualSpace">虚拟空间偏移量</param>
    /// <param name="allowInvalidPosition">是否允许无效的位置</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>始终返回Null，表示暂未实现具体的位置解析逻辑</returns>
    public Task<INavigableLocation?> GetLocationForPositionAsync(Workspace workspace, DocumentId documentId, int position, int virtualSpace, bool allowInvalidPosition, CancellationToken cancellationToken)
        => Task.FromResult<INavigableLocation?>(null);
}
