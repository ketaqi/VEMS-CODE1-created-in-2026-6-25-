namespace RoslynPad.Runtime;

/// <summary>
/// 定义控制台输出器的接口。
/// </summary>
internal interface IConsoleDumper
{
    /// <summary>
    /// 获取是否支持重定向标准输入/输出。
    /// </summary>
    bool SupportsRedirect { get; }

    /// <summary>
    /// 创建用于输出的文本写入器。
    /// </summary>
    /// <param name="header">输出标题。</param>
    /// <returns>文本写入器。</returns>
    TextWriter CreateWriter(string? header = null);

    /// <summary>
    /// 创建用于输入的文本读取器。
    /// </summary>
    /// <returns>文本读取器。</returns>
    TextReader CreateReader();

    /// <summary>
    /// 输出数据。
    /// </summary>
    /// <param name="data">要输出的数据。</param>
    void Dump(in DumpData data);

    /// <summary>
    /// 输出异常。
    /// </summary>
    /// <param name="exception">要输出的异常。</param>
    void DumpException(Exception exception);

    /// <summary>
    /// 输出进度信息。
    /// </summary>
    /// <param name="result">进度结果对象。</param>
    void DumpProgress(ProgressResultObject result);

    /// <summary>
    /// 刷新输出缓冲区。
    /// </summary>
    void Flush();
}
