using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeActions.WorkspaceServices;
using Microsoft.CodeAnalysis.Host.Mef;

namespace RoslynPad.Roslyn.WorkspaceServices;

/// <summary>
/// 用于创建添加元数据引用代码操作的工作区服务实现
/// </summary>
[ExportWorkspaceService(typeof(IAddMetadataReferenceCodeActionOperationFactoryWorkspaceService)), Shared]
internal sealed class AddMetadataReferenceCodeActionOperationFactoryWorkspaceService : IAddMetadataReferenceCodeActionOperationFactoryWorkspaceService
{
    /// <summary>
    /// 创建添加元数据引用的代码操作实例
    /// </summary>
    /// <param name="projectId">目标项目的唯一标识</param>
    /// <param name="assemblyIdentity">要添加的程序集标识信息</param>
    /// <returns>封装添加元数据引用逻辑的代码操作实例</returns>
    public CodeActionOperation CreateAddMetadataReferenceOperation(ProjectId projectId, AssemblyIdentity assemblyIdentity)
    {
        return new AddMetadataReferenceOperation(projectId, assemblyIdentity);
    }

    /// <summary>
    /// 具体实现添加元数据引用逻辑的代码操作类
    /// </summary>
    /// <param name="projectId">目标项目的唯一标识</param>
    /// <param name="assemblyIdentity">要添加的程序集标识信息</param>
    private class AddMetadataReferenceOperation(ProjectId projectId, AssemblyIdentity assemblyIdentity) : CodeActionOperation
    {
        private readonly AssemblyIdentity _assemblyIdentity = assemblyIdentity;
        private readonly ProjectId _projectId = projectId;

        /// <summary>
        /// 应用添加元数据引用的操作到指定工作区
        /// </summary>
        /// <param name="workspace">目标工作区实例</param>
        /// <param name="cancellationToken">取消操作的令牌</param>
        public override void Apply(Workspace workspace, CancellationToken cancellationToken)
        {
            // 将工作区转换为RoslynPad专用工作区，尝试添加元数据引用
            var roslynPadWorkspace = workspace as RoslynWorkspace;
            roslynPadWorkspace?.RoslynHost?.AddMetadataReference(_projectId, _assemblyIdentity);
        }

        /// <summary>
        /// 获取操作的显示标题
        /// </summary>
        public override string Title => "Add Reference";
    }
}
