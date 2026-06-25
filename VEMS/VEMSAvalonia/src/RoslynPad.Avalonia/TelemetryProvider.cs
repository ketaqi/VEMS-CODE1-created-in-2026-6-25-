using System.Composition;
using RoslynPad.UI;

namespace RoslynPad;

/// <summary>
/// 遥测提供器（Telemetry Provider）的 MEF 导出实现：
/// 用于向 RoslynPad 的 UI 层提供统一的埋点/事件上报能力。
/// </summary>
/// <remarks>
/// 本类仅作为 <see cref="ITelemetryProvider"/> 的具体导出类型存在，实际功能由基类
/// <see cref="TelemetryProviderBase"/> 提供或在其内部实现。<br/>
/// 通过 <c>[Export]</c> + <c>[Shared]</c>，确保在组合容器中以单例方式被解析并复用。
/// </remarks>
[Export(typeof(ITelemetryProvider)), Shared]
internal class TelemetryProvider : TelemetryProviderBase
{
}
