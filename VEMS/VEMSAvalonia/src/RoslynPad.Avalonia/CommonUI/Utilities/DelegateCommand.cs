using System.Windows.Input;

namespace RoslynPad.Utilities;

/// <summary>
/// 委托命令接口：扩展 <see cref="ICommand"/>，提供无参数执行能力和手动触发 CanExecute 变更通知。
/// </summary>
/// <remarks>
/// <para>
/// 此接口是 MVVM 模式中命令的增强版本，解决了 <see cref="ICommand"/> 接口的以下局限：
/// <list type="bullet">
///   <item><description>提供强类型的无参数 <see cref="Execute()"/> 和 <see cref="CanExecute()"/> 方法</description></item>
///   <item><description>支持手动触发 <see cref="RaiseCanExecuteChanged"/> 以通知 UI 刷新按钮状态</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IDelegateCommand : ICommand
{
    /// <summary>
    /// 执行命令。
    /// </summary>
    void Execute();

    /// <summary>
    /// 判断命令是否可执行。
    /// </summary>
    /// <returns>如果命令可执行返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    bool CanExecute();

    /// <summary>
    /// 触发 <see cref="ICommand.CanExecuteChanged"/> 事件，通知绑定的 UI 元素刷新可用状态。
    /// </summary>
    /// <remarks>
    /// 当影响命令可执行状态的条件发生变化时，应调用此方法。
    /// </remarks>
    void RaiseCanExecuteChanged();
}

/// <summary>
/// 泛型委托命令接口：支持带参数的命令执行。
/// </summary>
/// <typeparam name="T">命令参数的类型。</typeparam>
/// <remarks>
/// 此接口继承自 <see cref="IDelegateCommand"/>，并添加了带参数的执行方法。
/// 参数通常通过 XAML 中的 <c>CommandParameter</c> 属性传递。
/// </remarks>
public interface IDelegateCommand<in T> : IDelegateCommand
{
    /// <summary>
    /// 使用指定参数执行命令。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    void Execute(T parameter);

    /// <summary>
    /// 判断使用指定参数时命令是否可执行。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    /// <returns>如果命令可执行返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    bool CanExecute(T parameter);
}

/// <summary>
/// 委托命令实现：将 <see cref="Action"/> 或 <see cref="Func{Task}"/> 包装为 <see cref="ICommand"/>。
/// </summary>
/// <remarks>
/// <para>
/// 此类是 MVVM 模式中最常用的命令实现，支持：
/// <list type="bullet">
///   <item><description>同步执行（<see cref="Action"/>）</description></item>
///   <item><description>异步执行（<see cref="Func{Task}"/>）—— 以"发射后不管"方式运行</description></item>
///   <item><description>可选的可执行条件判断（<see cref="Func{Boolean}"/>）</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 同步命令
/// SaveCommand = new DelegateCommand(
///     () => SaveDocument(),
///     () => IsDirty
/// );
/// 
/// // 异步命令
/// LoadCommand = new DelegateCommand(
///     async () => await LoadDataAsync(),
///     () => !IsLoading
/// );
/// 
/// // 手动刷新按钮状态
/// IsDirty = true;
/// SaveCommand.RaiseCanExecuteChanged();
/// </code>
/// </example>
internal class DelegateCommand : IDelegateCommand
{
    /// <summary>
    /// 同步执行委托。
    /// </summary>
    private readonly Action? _action;

    /// <summary>
    /// 可执行条件判断委托。
    /// </summary>
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// 异步执行委托。
    /// </summary>
    private readonly Func<Task>? _asyncAction;

    /// <summary>
    /// 使用同步操作初始化 <see cref="DelegateCommand"/> 类的新实例。
    /// </summary>
    /// <param name="action">命令执行时调用的操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    public DelegateCommand(Action action, Func<bool>? canExecute = null)
    {
        _action = action;
        _canExecute = canExecute;
    }

    /// <summary>
    /// 使用异步操作初始化 <see cref="DelegateCommand"/> 类的新实例。
    /// </summary>
    /// <param name="asyncAction">命令执行时调用的异步操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    /// <remarks>
    /// 异步操作以"发射后不管"（fire-and-forget）方式执行，
    /// 即不等待任务完成。如需错误处理，应在委托内部捕获异常。
    /// </remarks>
    public DelegateCommand(Func<Task> asyncAction, Func<bool>? canExecute = null)
    {
        _asyncAction = asyncAction;
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public bool CanExecute() => _canExecute == null || _canExecute();

    /// <inheritdoc/>
    public void Execute()
    {
        if (_asyncAction != null)
        {
            _ = _asyncAction();
        }
        else
        {
            _action?.Invoke();
        }
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter) => CanExecute();

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter) => Execute();

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// 泛型委托命令实现：支持带参数的命令执行。
/// </summary>
/// <typeparam name="T">命令参数的类型。</typeparam>
/// <remarks>
/// <para>
/// 此类扩展了 <see cref="DelegateCommand"/>，支持从 XAML 的 <c>CommandParameter</c> 接收参数。
/// </para>
/// <para>
/// 参数转换规则：
/// <list type="bullet">
///   <item><description>如果 <typeparamref name="T"/> 是值类型且参数为 <c>null</c>，使用 <c>default(T)</c></description></item>
///   <item><description>否则直接转换为 <typeparamref name="T"/></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 带参数的命令
/// DeleteItemCommand = new DelegateCommand&lt;ItemViewModel&gt;(
///     item => Items.Remove(item),
///     item => item != null &amp;&amp; Items.Contains(item)
/// );
/// 
/// // 在 XAML 中使用
/// // &lt;Button Command="{Binding DeleteItemCommand}" 
/// //         CommandParameter="{Binding SelectedItem}" /&gt;
/// </code>
/// </example>
internal class DelegateCommand<T> : IDelegateCommand<T>
{
    /// <summary>
    /// 同步执行委托。
    /// </summary>
    private readonly Action<T?>? _action;

    /// <summary>
    /// 可执行条件判断委托。
    /// </summary>
    private readonly Func<T?, bool>? _canExecute;

    /// <summary>
    /// 异步执行委托。
    /// </summary>
    private readonly Func<T?, Task>? _asyncAction;

    /// <summary>
    /// 使用同步操作初始化 <see cref="DelegateCommand{T}"/> 类的新实例。
    /// </summary>
    /// <param name="action">命令执行时调用的操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    public DelegateCommand(Action<T?> action, Func<T?, bool>? canExecute = null)
    {
        _action = action;
        _canExecute = canExecute;
    }

    /// <summary>
    /// 使用异步操作初始化 <see cref="DelegateCommand{T}"/> 类的新实例。
    /// </summary>
    /// <param name="asyncAction">命令执行时调用的异步操作。</param>
    /// <param name="canExecute">判断命令是否可执行的条件；<c>null</c> 表示始终可执行。</param>
    public DelegateCommand(Func<T?, Task> asyncAction, Func<T?, bool>? canExecute = null)
    {
        _asyncAction = asyncAction;
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public bool CanExecute(T? parameter) => _canExecute == null || _canExecute(parameter);

    /// <inheritdoc/>
    public void Execute(T? parameter)
    {
        if (_asyncAction != null)
        {
            _ = _asyncAction(parameter);
        }
        else
        {
            _action?.Invoke(parameter);
        }
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter) => CanExecute(GetParameter(parameter));

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter) => Execute(GetParameter(parameter));

    /// <summary>
    /// 将 <see cref="object"/> 类型的参数转换为 <typeparamref name="T"/>。
    /// </summary>
    /// <param name="parameter">原始参数。</param>
    /// <returns>转换后的参数值。</returns>
    /// <remarks>
    /// 如果 <typeparamref name="T"/> 是值类型且 <paramref name="parameter"/> 为 <c>null</c>，
    /// 返回 <c>default(T)</c> 以避免装箱/拆箱异常。
    /// </remarks>
    private static T? GetParameter(object? parameter) =>
        typeof(T).IsValueType && parameter == null ? default : (T)parameter!;

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    void IDelegateCommand.Execute() => Execute(default!);

    /// <inheritdoc/>
    bool IDelegateCommand.CanExecute() => CanExecute(default!);
}
