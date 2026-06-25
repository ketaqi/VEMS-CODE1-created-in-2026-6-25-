using Microsoft.CodeAnalysis.PasteTracking;
using Microsoft.CodeAnalysis.Text;
using System.Composition;

namespace RoslynPad.Roslyn.CodeRefactorings;

/// <summary>
/// 粘贴跟踪服务实现类，用于处理粘贴文本的范围跟踪
/// </summary>
[Export(typeof(IPasteTrackingService)), Shared]
internal class PasteTrackingService : IPasteTrackingService
{
    /// <summary>
    /// 尝试获取粘贴文本的范围
    /// </summary>
    /// <param name="sourceTextContainer">源文本容器</param>
    /// <param name="textSpan">输出参数，粘贴文本的范围（若获取成功）</param>
    /// <returns>成功获取返回true，否则返回false</returns>
    public bool TryGetPastedTextSpan(SourceTextContainer sourceTextContainer, out TextSpan textSpan)
    {
        textSpan = default;
        return false;
    }
}
