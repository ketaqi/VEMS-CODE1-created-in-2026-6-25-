using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.QuickInfo;

/// <summary>
/// 快速信息（Quick Info）提供器接口，定义获取快速信息项的契约
/// </summary>
public interface IQuickInfoProvider
{
    /// <summary>
    /// 异步获取指定文档中指定位置的快速信息项
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="position">文档中的字符位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>快速信息项，若不存在则返回null</returns>
    Task<QuickInfoItem?> GetItemAsync(Document document, int position, CancellationToken cancellationToken = default);
}
