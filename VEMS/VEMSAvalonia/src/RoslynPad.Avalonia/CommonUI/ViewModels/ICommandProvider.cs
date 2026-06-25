using System.Composition;
using RoslynPad.Utilities;

namespace RoslynPad.UI;

/// <summary>
/// 命令提供者接口：工厂模式创建 <see cref="IDelegateCommand"/> 实例。
/// </summary>
/// <remarks>
/// <para>
/// 此接口抽象了命令的创建过程，使得 ViewModel 不直接依赖具体的命令实现类。
/// 通过依赖注入获取 <see cref="ICommandProvider"/> 实例，可以：
/// <list type="bullet">
///   <item><description>简化命令创建的样板代码</description></item>
///   <item><description>支持单元测试中的命令模拟</description></item>
///   <item><description>统一管理命令的创建策略</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyViewModel
/// {
///     public IDelegateCommand SaveCommand { get; }
///     public IDelegateCommand&lt;string&gt; OpenCommand { get; }
///     
///     public MyViewModel(ICommandProvider commands)
///     {
///         SaveCommand = commands.Create(Save, () => IsDirty);
///         OpenCommand = commands.CreateAsync&lt;string&gt;(OpenFileAsync);
///     }
/// }
/// </code>
/// </example>
public interface ICommandProvider
{
    /// <summary>
    /// 创建一个同步执行的无参数命令。
    /// </summary>
    /// <param name="execute">命令执行时调用的操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    /// <returns>创建的命令实例。</returns>
    IDelegateCommand Create(Action execute, Func<bool>? canExecute = null);

    /// <summary>
    /// 创建一个异步执行的无参数命令。
    /// </summary>
    /// <param name="execute">命令执行时调用的异步操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    /// <returns>创建的命令实例。</returns>
    /// <remarks>
    /// 异步操作以"发射后不管"方式执行，即方法立即返回，不等待任务完成。
    /// </remarks>
    IDelegateCommand CreateAsync(Func<Task> execute, Func<bool>? canExecute = null);

    /// <summary>
    /// 创建一个同步执行的带参数命令。
    /// </summary>
    /// <typeparam name="T">命令参数的类型。</typeparam>
    /// <param name="execute">命令执行时调用的操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    /// <returns>创建的命令实例。</returns>
    IDelegateCommand<T> Create<T>(Action<T?> execute, Func<T?, bool>? canExecute = null);

    /// <summary>
    /// 创建一个异步执行的带参数命令。
    /// </summary>
    /// <typeparam name="T">命令参数的类型。</typeparam>
    /// <param name="execute">命令执行时调用的异步操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    /// <returns>创建的命令实例。</returns>
    IDelegateCommand<T> CreateAsync<T>(Func<T?, Task> execute, Func<T?, bool>? canExecute = null);
}

/// <summary>
/// 命令提供者实现：使用 <see cref="DelegateCommand"/> 创建命令实例。
/// </summary>
/// <remarks>
/// <para>
/// 此类通过 MEF 容器导出为 <see cref="ICommandProvider"/> 的单例实现。
/// 所有通过此提供者创建的命令都使用 <see cref="DelegateCommand"/> 或 <see cref="DelegateCommand{T}"/>。
/// </para>
/// </remarks>
[Export(typeof(ICommandProvider)), Shared]
internal class CommandProvider : ICommandProvider
{
    /// <inheritdoc/>
    public IDelegateCommand Create(Action execute, Func<bool>? canExecute = null)
    {
        return new DelegateCommand(execute, canExecute);
    }

    /// <inheritdoc/>
    public IDelegateCommand CreateAsync(Func<Task> execute, Func<bool>? canExecute = null)
    {
        return new DelegateCommand(execute, canExecute);
    }

    /// <inheritdoc/>
    public IDelegateCommand<T> Create<T>(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        return new DelegateCommand<T>(execute, canExecute);
    }

    /// <inheritdoc/>
    public IDelegateCommand<T> CreateAsync<T>(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
    {
        return new DelegateCommand<T>(execute, canExecute);
    }
}
