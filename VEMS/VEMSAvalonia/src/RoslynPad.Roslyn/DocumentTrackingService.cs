using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace RoslynPad.Roslyn;

/// <summary>
/// 文档跟踪服务工厂，用于创建IDocumentTrackingService实例
/// </summary>
[ExportWorkspaceServiceFactory(typeof(IDocumentTrackingService), ServiceLayer.Host)]
internal sealed class DocumentTrackingServiceFactory : IWorkspaceServiceFactory
{
    /// <summary>
    /// 文档跟踪服务实现类，负责跟踪工作区中的活动文档和可见文档
    /// </summary>
    /// <param name="workspace">所属的工作区实例</param>
    private class DocumentTrackingService(Workspace workspace) : IDocumentTrackingService
    {
        private readonly RoslynWorkspace _workspace = (RoslynWorkspace)workspace;

        /// <summary>
        /// 获取一个值，指示当前服务是否支持文档跟踪
        /// </summary>
        public bool SupportsDocumentTracking => true;

        /// <summary>
        /// 获取当前活动的文档ID
        /// </summary>
        /// <returns>当前活动文档的ID</returns>
        /// <exception cref="InvalidOperationException">当没有活动文档时抛出</exception>
        public DocumentId GetActiveDocument() => _workspace.OpenDocumentId ?? throw new InvalidOperationException("No active document");

        /// <summary>
        /// 尝试获取当前活动的文档ID
        /// </summary>
        /// <returns>当前活动文档的ID（如果存在），否则返回null</returns>
        public DocumentId? TryGetActiveDocument() => _workspace.OpenDocumentId;

        /// <summary>
        /// 获取当前可见的文档ID列表
        /// </summary>
        /// <returns>包含可见文档ID的不可变数组</returns>
        public ImmutableArray<DocumentId> GetVisibleDocuments() => _workspace.OpenDocumentId != null ? [_workspace.OpenDocumentId] : [];

        /// <summary>
        /// 当活动文档发生变化时触发的事件
        /// </summary>
        public event EventHandler<DocumentId?>? ActiveDocumentChanged = delegate { };

        // public event EventHandler<EventArgs>? NonRoslynBufferTextChanged = delegate { };
    }

    /// <summary>
    /// 创建文档跟踪服务实例
    /// </summary>
    /// <param name="workspaceServices">工作区服务容器</param>
    /// <returns>创建的IWorkspaceService实例</returns>
    public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices) =>
        new DocumentTrackingService(workspaceServices.Workspace);
}
