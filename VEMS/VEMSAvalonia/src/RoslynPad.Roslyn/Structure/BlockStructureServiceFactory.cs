using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Structure;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynPad.Roslyn.Structure;

/// <summary>
/// C#语言的块结构服务工厂，用于创建<see cref="IBlockStructureService"/>实例
/// </summary>
[ExportLanguageServiceFactory(typeof(IBlockStructureService), LanguageNames.CSharp), Shared]
internal class BlockStructureServiceFactory : ILanguageServiceFactory
{
    /// <summary>
    /// 创建指定语言服务的实例
    /// </summary>
    /// <param name="languageServices">语言服务上下文</param>
    /// <returns>实现<see cref="IBlockStructureService"/>的包装类实例</returns>
    public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        => new BlockStructureServiceWrapper(languageServices.LanguageServices.GetRequiredService<BlockStructureService>());

    /// <summary>
    /// <see cref="IBlockStructureService"/>的包装类，适配Roslyn原生块结构服务
    /// </summary>
    /// <param name="service">Roslyn原生的块结构服务实例</param>
    private class BlockStructureServiceWrapper(BlockStructureService service) : ILanguageService, IBlockStructureService
    {
        /// <inheritdoc/>
        public async Task<BlockStructure> GetBlockStructureAsync(Document document, CancellationToken cancellationToken)
        {
            return new(await service.GetBlockStructureAsync(document, BlockStructureOptions.Default, cancellationToken).ConfigureAwait(false));
        }
    }
}
