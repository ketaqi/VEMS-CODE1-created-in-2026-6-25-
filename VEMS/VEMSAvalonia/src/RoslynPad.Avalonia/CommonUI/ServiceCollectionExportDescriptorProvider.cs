using System.Composition.Hosting.Core;
using Microsoft.Extensions.DependencyInjection;

namespace RoslynPad;

/// <summary>
/// 服务集合导出描述符提供者：将 <see cref="ServiceCollection"/> 桥接到 MEF 容器。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现 <see cref="ExportDescriptorProvider"/>，允许通过 MEF 容器解析
/// 在 <see cref="ServiceCollection"/> 中注册的服务。
/// </para>
/// <para>
/// 这使得可以在同一个应用程序中混合使用 MEF 和 Microsoft.Extensions.DependencyInjection：
/// <list type="bullet">
///   <item><description>通过 MEF 自动发现和组合带有 <c>[Export]</c> 特性的类型</description></item>
///   <item><description>通过 <see cref="ServiceCollection"/> 手动注册服务</description></item>
/// </list>
/// </para>
/// <para>
/// 生命周期映射：
/// <list type="bullet">
///   <item><description><see cref="ServiceLifetime.Singleton"/> / <see cref="ServiceLifetime.Scoped"/> → 共享实例</description></item>
///   <item><description><see cref="ServiceLifetime.Transient"/> → 非共享实例</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var services = new ServiceCollection();
/// services.AddSingleton&lt;ILogger, ConsoleLogger&gt;();
/// services.AddTransient&lt;IMyService, MyService&gt;();
/// 
/// var configuration = new ContainerConfiguration()
///     .WithProvider(new ServiceCollectionExportDescriptorProvider(services))
///     .WithAssembly(typeof(App).Assembly);
/// 
/// using var container = configuration.CreateContainer();
/// var logger = container.GetExport&lt;ILogger&gt;(); // 从 ServiceCollection 解析
/// </code>
/// </example>
/// <param name="services">要桥接的服务集合。</param>
public class ServiceCollectionExportDescriptorProvider(ServiceCollection services) : ExportDescriptorProvider
{
    /// <summary>
    /// 服务类型到服务描述符的映射（每个类型只保留最后注册的服务）。
    /// </summary>
    private readonly Dictionary<Type, ServiceDescriptor> _services = services
        .GroupBy(s => s.ServiceType)
        .Select(s => s.Last())
        .ToDictionary(s => s.ServiceType);

    /// <summary>
    /// 用于解析服务的内部服务提供者。
    /// </summary>
    private readonly ServiceProvider _serviceProvider = services.BuildServiceProvider();

    /// <summary>
    /// 获取满足指定契约的导出描述符。
    /// </summary>
    /// <param name="contract">要满足的组合契约。</param>
    /// <param name="descriptorAccessor">用于访问其他导出描述符的访问器。</param>
    /// <returns>匹配的导出描述符承诺枚举；如果无匹配则返回空枚举。</returns>
    /// <remarks>
    /// <para>
    /// 此方法处理以下情况：
    /// <list type="bullet">
    ///   <item><description>精确类型匹配</description></item>
    ///   <item><description>泛型类型定义匹配（如 <c>ILogger&lt;T&gt;</c>）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(
        CompositionContract contract,
        DependencyAccessor descriptorAccessor)
    {
        if (!_services.TryGetValue(contract.ContractType, out var service) &&
            !(contract.ContractType.IsGenericType &&
              contract.ContractType.GetGenericTypeDefinition() is var genericType &&
              _services.TryGetValue(genericType, out service)))
        {
            yield break;
        }

        yield return new ExportDescriptorPromise(
            contract,
            nameof(ServiceCollectionExportDescriptorProvider),
            service.Lifetime != ServiceLifetime.Transient,
            Array.Empty<CompositionDependency>,
            _ => ExportDescriptor.Create(
                (_, _) => _serviceProvider.GetService(contract.ContractType),
                new Dictionary<string, object>()));
    }
}
