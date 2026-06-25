using System;
using System.Windows.Input;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// 泛型中继命令实现，用于 MVVM 模式中的命令绑定。
    /// </summary>
    /// <typeparam name="T">命令参数的类型。</typeparam>
    /// <remarks>
    /// 此类将委托包装为 <see cref="ICommand"/>，简化视图模型中的命令实现。
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// public ICommand DeleteCommand { get; } = new RelayCommand&lt;Item&gt;(
    ///     item => DeleteItem(item),
    ///     item => item != null);
    /// </code>
    /// </example>
    public class RelayCommand<T> : ICommand
    {
        /// <summary>
        /// 执行命令的委托。
        /// </summary>
        private readonly Action<T?> _execute;

        /// <summary>
        /// 判断命令是否可执行的委托。
        /// </summary>
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// 初始化 <see cref="RelayCommand{T}"/> 类的新实例。
        /// </summary>
        /// <param name="execute">执行命令时调用的委托。</param>
        /// <param name="canExecute">判断命令是否可执行的委托（可选）。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="execute"/> 为 <see langword="null"/> 时抛出。
        /// </exception>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 当命令的可执行状态变化时触发。
        /// </summary>
        /// <remarks>
        /// 此实现为空，因为此简单版本不支持自动刷新。
        /// 如需支持，请使用 CommandManager.RequerySuggested。
        /// </remarks>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// 确定命令是否可以在当前状态下执行。
        /// </summary>
        /// <param name="parameter">命令参数。</param>
        /// <returns>若可执行返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <param name="parameter">命令参数。</param>
        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }
    }

    /// <summary>
    /// 非泛型中继命令实现，用于 MVVM 模式中的命令绑定。
    /// </summary>
    /// <remarks>
    /// 此类是 <see cref="RelayCommand{T}"/> 的非泛型版本，
    /// 适用于不需要特定参数类型的命令。
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// public ICommand SaveCommand { get; } = new RelayCommand(
    ///     _ => Save(),
    ///     _ => CanSave);
    /// </code>
    /// </example>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// 执行命令的委托。
        /// </summary>
        private readonly Action<object?> _execute;

        /// <summary>
        /// 判断命令是否可执行的委托。
        /// </summary>
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// 初始化 <see cref="RelayCommand"/> 类的新实例。
        /// </summary>
        /// <param name="execute">执行命令时调用的委托。</param>
        /// <param name="canExecute">判断命令是否可执行的委托（可选）。</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 确定命令是否可以在当前状态下执行。
        /// </summary>
        /// <param name="parameter">命令参数。</param>
        /// <returns>若可执行返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <param name="parameter">命令参数。</param>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// 当命令的可执行状态变化时触发。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
