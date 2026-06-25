using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynPad.Roslyn.Structure;

/// <summary>
/// 定义块结构服务的接口，用于获取文档的代码块结构信息
/// </summary>
public interface IBlockStructureService : ILanguageService
{
    /// <summary>
    /// 异步获取指定文档的块结构信息
    /// </summary>
    /// <param name="document">要分析的文档实例</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>包含文档块结构的<see cref="BlockStructure"/>实例</returns>
    Task<BlockStructure> GetBlockStructureAsync(Document document, CancellationToken cancellationToken = default);
}
