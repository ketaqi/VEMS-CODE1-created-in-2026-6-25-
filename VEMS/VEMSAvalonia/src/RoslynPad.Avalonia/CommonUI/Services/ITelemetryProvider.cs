namespace RoslynPad.UI;

/// <summary>
/// 遥测提供者接口：定义错误报告和遥测数据收集的契约。
/// </summary>
/// <remarks>
/// <para>
/// 此接口用于统一管理应用程序的错误报告和遥测功能。
/// 支持捕获未处理异常、记录错误日志，并可选地发送到远程服务。
/// </para>
/// <para>
/// 隐私说明：遥测功能应尊重用户选择，可通过 <see cref="IApplicationSettingsValues.SendErrors"/> 控制。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 初始化遥测
/// var telemetry = serviceProvider.GetRequiredService&lt;ITelemetryProvider&gt;();
/// telemetry.Initialize("1.0.0", applicationSettings);
/// 
/// // 报告异常
/// try
/// {
///     await SomeOperationAsync();
/// }
/// catch (Exception ex)
/// {
///     telemetry.ReportError(ex);
/// }
/// 
/// // 监听错误变化以更新 UI
/// telemetry.LastErrorChanged += () =>
/// {
///     if (telemetry.LastError != null)
///     {
///         ShowErrorNotification(telemetry.LastError.Message);
///     }
/// };
/// </code>
/// </example>
public interface ITelemetryProvider
{
    /// <summary>
    /// 初始化遥测服务。
    /// </summary>
    /// <param name="version">应用程序版本号，如 "1.0.0"。</param>
    /// <param name="settings">应用程序设置，用于读取遥测配置（如是否启用错误报告）。</param>
    /// <remarks>
    /// 此方法应在应用程序启动时调用一次，配置遥测服务的运行参数。
    /// </remarks>
    void Initialize(string version, IApplicationSettings settings);

    /// <summary>
    /// 获取最近发生的错误。
    /// </summary>
    /// <value>
    /// 最后一个通过 <see cref="ReportError"/> 报告的异常；如果没有错误或已清除，则为 <c>null</c>。
    /// </value>
    /// <remarks>
    /// 此属性可用于在 UI 中显示错误提示栏。
    /// </remarks>
    Exception? LastError { get; }

    /// <summary>
    /// 当 <see cref="LastError"/> 发生变化时触发。
    /// </summary>
    /// <remarks>
    /// 订阅此事件以在 UI 中响应错误状态变化，
    /// 例如显示或隐藏错误提示栏。
    /// </remarks>
    event Action LastErrorChanged;

    /// <summary>
    /// 清除最近的错误记录。
    /// </summary>
    /// <remarks>
    /// 调用此方法后，<see cref="LastError"/> 将变为 <c>null</c>，
    /// 并触发 <see cref="LastErrorChanged"/> 事件。
    /// 通常由用户点击"关闭错误提示"按钮触发。
    /// </remarks>
    void ClearLastError();

    /// <summary>
    /// 报告一个错误。
    /// </summary>
    /// <param name="exception">要报告的异常对象。</param>
    /// <remarks>
    /// <para>
    /// 调用此方法将：
    /// <list type="number">
    ///   <item><description>更新 <see cref="LastError"/> 属性</description></item>
    ///   <item><description>触发 <see cref="LastErrorChanged"/> 事件</description></item>
    ///   <item><description>如果启用了遥测，可能将错误发送到远程服务</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await CompileAndRunAsync();
    /// }
    /// catch (CompilationException ex)
    /// {
    ///     _telemetry.ReportError(ex);
    /// }
    /// </code>
    /// </example>
    void ReportError(Exception exception);
}
