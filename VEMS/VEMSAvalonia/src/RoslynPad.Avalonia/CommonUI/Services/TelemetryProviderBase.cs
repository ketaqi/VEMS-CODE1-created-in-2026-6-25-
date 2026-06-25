namespace RoslynPad.UI;

/// <summary>
/// 遥测提供者基类：实现 <see cref="ITelemetryProvider"/> 接口的默认行为。
/// </summary>
/// <remarks>
/// <para>
/// 此抽象基类提供遥测功能的通用实现，包括：
/// <list type="bullet">
///   <item><description>自动订阅未处理异常事件（<see cref="AppDomain.UnhandledException"/> 和 <see cref="TaskScheduler.UnobservedTaskException"/>）</description></item>
///   <item><description>统一的错误处理与存储机制</description></item>
///   <item><description>错误状态变更通知</description></item>
/// </list>
/// </para>
/// <para>
/// 派生类可重写 <see cref="Initialize"/> 方法添加额外的初始化逻辑（如远程错误上报服务配置）。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 自定义遥测提供者
/// public class AppTelemetryProvider : TelemetryProviderBase
/// {
///     public override void Initialize(string version, IApplicationSettings settings)
///     {
///         base.Initialize(version, settings);
///         
///         // 配置远程错误上报（如 Sentry、AppCenter 等）
///         if (settings.Values.SendErrors)
///         {
///             ConfigureRemoteReporting(version);
///         }
///     }
/// }
/// </code>
/// </example>
public abstract class TelemetryProviderBase : ITelemetryProvider
{
    /// <summary>
    /// 存储最近一次报告的错误。
    /// </summary>
    private Exception? _lastError;

    /// <summary>
    /// 初始化遥测服务，订阅应用程序域和任务调度器的未处理异常事件。
    /// </summary>
    /// <param name="version">应用程序版本号。</param>
    /// <param name="settings">应用程序设置（可用于检查是否启用遥测）。</param>
    /// <remarks>
    /// <para>
    /// 此方法会自动订阅以下事件：
    /// <list type="bullet">
    ///   <item><description><see cref="AppDomain.UnhandledException"/> - 捕获同步代码中的未处理异常</description></item>
    ///   <item><description><see cref="TaskScheduler.UnobservedTaskException"/> - 捕获未观察的任务异常</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 派生类可重写此方法以添加额外的初始化逻辑，但应调用基类实现以确保事件订阅正常工作。
    /// </para>
    /// </remarks>
    public virtual void Initialize(string version, IApplicationSettings settings)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
    }

    /// <summary>
    /// 处理任务调度器中未观察的异常。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="args">包含未观察异常的事件参数。</param>
    /// <remarks>
    /// 异常会被展平（Flatten）以获取真正的内部异常后传递给 <see cref="HandleException"/>。
    /// </remarks>
    private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        HandleException(args.Exception!.Flatten().InnerException!);
    }

    /// <summary>
    /// 处理应用程序域中的未处理异常。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="args">包含异常对象的事件参数。</param>
    private void CurrentDomainOnUnhandledException(object? sender, UnhandledExceptionEventArgs args)
    {
        HandleException((Exception)args.ExceptionObject);
    }

    /// <summary>
    /// 统一处理异常：过滤已取消的操作异常，并更新 <see cref="LastError"/> 状态。
    /// </summary>
    /// <param name="exception">要处理的异常。</param>
    /// <remarks>
    /// <para>
    /// 此方法会忽略 <see cref="OperationCanceledException"/>，
    /// 因为这类异常通常是用户主动取消操作的正常结果，不应作为错误报告。
    /// </para>
    /// <para>
    /// 派生类可重写此方法以添加自定义处理逻辑（如发送到远程服务器）。
    /// </para>
    /// </remarks>
    protected void HandleException(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return;
        }

        LastError = exception;
    }

    /// <summary>
    /// 报告一个错误。
    /// </summary>
    /// <param name="exception">要报告的异常对象。</param>
    /// <remarks>
    /// 此方法委托给 <see cref="HandleException"/> 进行处理，
    /// 适用于主动捕获并上报异常的场景。
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await CompileAsync();
    /// }
    /// catch (CompilationException ex)
    /// {
    ///     _telemetry.ReportError(ex);
    /// }
    /// </code>
    /// </example>
    public void ReportError(Exception exception)
    {
        HandleException(exception);
    }

    /// <summary>
    /// 获取最近发生的错误。
    /// </summary>
    /// <value>
    /// 最后一个被处理的异常；如果没有错误或已调用 <see cref="ClearLastError"/>，则为 <c>null</c>。
    /// </value>
    /// <remarks>
    /// 设置此属性时会自动触发 <see cref="LastErrorChanged"/> 事件，
    /// 用于通知 UI 更新错误显示状态。
    /// </remarks>
    public Exception? LastError
    {
        get => _lastError;
        private set
        {
            _lastError = value;
            LastErrorChanged?.Invoke();
        }
    }

    /// <summary>
    /// 当 <see cref="LastError"/> 发生变化时触发。
    /// </summary>
    /// <remarks>
    /// 订阅此事件以在 UI 中响应错误状态变化，
    /// 例如显示或隐藏错误提示栏。
    /// </remarks>
    public event Action? LastErrorChanged;

    /// <summary>
    /// 清除最近的错误记录。
    /// </summary>
    /// <remarks>
    /// 调用此方法后，<see cref="LastError"/> 将变为 <c>null</c>，
    /// 并触发 <see cref="LastErrorChanged"/> 事件。
    /// 通常由用户点击"关闭错误提示"按钮触发。
    /// </remarks>
    public void ClearLastError()
    {
        LastError = null;
    }
}
