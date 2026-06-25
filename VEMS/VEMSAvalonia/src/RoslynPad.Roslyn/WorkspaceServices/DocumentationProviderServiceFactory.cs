using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Composition;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.WorkspaceServices;

/// <summary>
/// 文档提供程序服务的工厂类，用于创建IDocumentationProviderService实例
/// </summary>
/// <param name="service">注入的文档提供程序服务实例</param>
[ExportWorkspaceServiceFactory(typeof(IDocumentationProviderService), ServiceLayer.Host), Shared]
[method: ImportingConstructor]
internal sealed class DocumentationProviderServiceFactory(IDocumentationProviderService service) : IWorkspaceServiceFactory
{
    private readonly IDocumentationProviderService _service = service;

    /// <summary>
    /// 创建工作区服务实例
    /// </summary>
    /// <param name="workspaceServices">宿主工作区服务集合</param>
    /// <returns>文档提供程序服务实例</returns>
    public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices) => _service;
}

/// <summary>
/// 文档提供程序服务的具体实现类，负责管理和提供程序集的XML文档提供程序
/// </summary>
[Export(typeof(IDocumentationProviderService)), Shared]
internal sealed class DocumentationProviderService : IDocumentationProviderService
{
    // 缓存程序集路径到文档提供程序的映射，避免重复创建
    private readonly ConcurrentDictionary<string, DocumentationProvider> _assemblyPathToDocumentationProviderMap = new();

    /// <summary>
    /// 根据程序集路径获取对应的文档提供程序
    /// </summary>
    /// <param name="location">程序集文件的路径</param>
    /// <returns>对应程序集的文档提供程序，不存在则返回默认提供程序</returns>
    public DocumentationProvider GetDocumentationProvider(string location)
    {
        // 将程序集路径替换为对应的XML文档路径
        string? finalPath = Path.ChangeExtension(location, "xml");

        // 从缓存获取或创建文档提供程序
        return _assemblyPathToDocumentationProviderMap.GetOrAdd(location, _ =>
        {
            // 如果XML文档文件不存在，返回默认文档提供程序
            if (!File.Exists(finalPath))
            {
                return DocumentationProvider.Default;
            }

            // 从XML文件创建文档提供程序
            return XmlDocumentationProvider.CreateFromFile(finalPath);
        });
    }
}
