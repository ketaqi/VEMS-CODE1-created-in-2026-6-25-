using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace RoslynPad.Roslyn;

/// <summary>
/// Roslyn宿主接口，定义Roslyn工作区的核心操作契约
/// </summary>
public interface IRoslynHost
{
    /// <summary>
    /// 获取解析选项
    /// </summary>
    ParseOptions ParseOptions { get; }

    /// <summary>
    /// 获取指定类型的服务实例
    /// </summary>
    /// <typeparam name="TService">服务类型</typeparam>
    /// <returns>服务实例</returns>
    TService GetService<TService>();

    /// <summary>
    /// 根据文档ID获取指定类型的工作区服务
    /// </summary>
    /// <typeparam name="TService">工作区服务类型（需实现 <see cref="IWorkspaceService"/>）</typeparam>
    /// <param name="documentId">文档唯一标识</param>
    /// <returns>工作区服务实例</returns>
    TService GetWorkspaceService<TService>(DocumentId documentId) where TService : IWorkspaceService;

    /// <summary>
    /// 添加新文档到Roslyn工作区
    /// </summary>
    /// <param name="args">文档创建参数</param>
    /// <returns>新建文档的唯一标识</returns>
    DocumentId AddDocument(DocumentCreationArgs args);

    /// <summary>
    /// 根据文档ID获取文档对象
    /// </summary>
    /// <param name="documentId">文档唯一标识</param>
    /// <returns>文档对象，不存在则返回 null</returns>
    Document? GetDocument(DocumentId documentId);

    /// <summary>
    /// 关闭指定ID的文档
    /// </summary>
    /// <param name="documentId">文档唯一标识</param>
    void CloseDocument(DocumentId documentId);

    /// <summary>
    /// 根据文件路径创建元数据引用
    /// </summary>
    /// <param name="location">元数据文件路径</param>
    /// <returns>创建的元数据引用</returns>
    MetadataReference CreateMetadataReference(string location);
}
