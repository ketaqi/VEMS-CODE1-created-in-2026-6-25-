using System.Composition;

namespace RoslynPad.UI;

/// <summary>
/// 系统组合服务提供者：将 MEF (System.Composition) 容器适配为 <see cref="IServiceProvider"/>。
/// </summary>
/// <remarks>
/// <para>
/// 此类作为依赖注入容器的桥接适配器，允许通过标准的 <see cref="IServiceProvider"/> 接口
/// 访问 MEF 容器中注册的服务。
/// </para>
/// <para>
/// 使用场景：
/// <list type="bullet">
///   <item><description>在不直接依赖 MEF API 的代码中获取服务</description></item>
///   <item><description>与其他依赖注入框架或库集成</description></item>
///   <item><description>在 ViewModel 或服务类中通过构造函数注入</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // MEF 自动注入 CompositionContext
/// // 然后可通过 IServiceProvider 获取服务
/// var dialog = serviceProvider.GetService(typeof(ISaveFileDialog)) as ISaveFileDialog;
/// 
/// // 或使用扩展方法（需要 Microsoft.Extensions.DependencyInjection.Abstractions）
/// var dialog = serviceProvider.GetRequiredService&lt;ISaveFileDialog&gt;();
/// </code>
/// </example>
/// <param name="host">MEF 组合上下文，提供导出服务的解析能力。</param>
[Export(typeof(IServiceProvider)), Shared]
[method: ImportingConstructor]
internal class SystemCompositionServiceProvider(CompositionContext host) : IServiceProvider
{
    /// <summary>
    /// MEF 组合上下文，用于解析导出的服务。
    /// </summary>
    private readonly CompositionContext _host = host;

    /// <summary>
    /// 获取指定类型的服务实例。
    /// </summary>
    /// <param name="serviceType">要获取的服务类型。</param>
    /// <returns>
    /// 如果容器中存在该类型的导出，则返回服务实例；
    /// 否则可能抛出 <see cref="CompositionFailedException"/>。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 此方法委托给 <see cref="CompositionContext.GetExport(Type)"/> 进行服务解析。
    /// 如果服务不存在，将抛出异常而非返回 <c>null</c>。
    /// </para>
    /// <para>
    /// 若需安全获取（允许返回 null），请使用 <c>TryGetExport</c> 或捕获异常。
    /// </para>
    /// </remarks>
    /// <exception cref="CompositionFailedException">
    /// 当指定类型的服务未在容器中注册时抛出。
    /// </exception>
    public object GetService(Type serviceType)
    {
        return _host.GetExport(serviceType);
    }
}
