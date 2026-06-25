#if !NET6_0_OR_GREATER
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml;

namespace RoslynPad.Runtime;

/// <summary>
/// 使用 DataContractJsonSerializer 的 JSON 控制台输出器（.NET Standard 2.0/.NET Framework）。
/// </summary>
/// <remarks>
/// 此程序集不应有外部依赖，因此使用传统的 JSON 写入器（基于 XmlDictionaryWriter）。
/// 对于 .NET 6+，请参阅 SystemTextJsonConsoleDumper.cs。
/// </remarks>
internal class JsonConsoleDumper : IConsoleDumper, IDisposable
{
    /// <summary>
    /// 每个会话允许的最大输出次数，防止无限循环导致输出过多。
    /// </summary>
    private const int MaxDumpsPerSession = 100000;

    private static readonly byte[] s_newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

    private static readonly byte[] s_resultObjectHeader = Encoding.UTF8.GetBytes("o:");
    private static readonly byte[] s_exceptionResultHeader = Encoding.UTF8.GetBytes("e:");
    private static readonly byte[] s_inputReadRequestHeader = Encoding.UTF8.GetBytes("i:");
    private static readonly byte[] s_progressResultHeader = Encoding.UTF8.GetBytes("p:");

    private readonly Stream _stream;
    private readonly object _lock;
    private int _dumpCount;

    /// <summary>
    /// 初始化 <see cref="JsonConsoleDumper"/> 类的新实例。
    /// </summary>
    public JsonConsoleDumper()
    {
        _stream = Console.OpenStandardOutput();
        _lock = new object();
    }

    /// <summary>
    /// 创建基于 XmlDictionaryWriter 的 JSON 写入器。
    /// </summary>
    /// <remarks>
    /// 使用 JsonReaderWriterFactory 创建写入器，这是 WCF 数据契约序列化的底层实现。
    /// </remarks>
    private XmlDictionaryWriter CreateJsonWriter() =>
        JsonReaderWriterFactory.CreateJsonWriter(_stream, Encoding.UTF8, ownsStream: false);

    /// <inheritdoc/>
    public bool SupportsRedirect => true;

    /// <inheritdoc/>
    public TextWriter CreateWriter(string? header = null) => new ConsoleRedirectWriter(this, header);

    /// <inheritdoc/>
    public TextReader CreateReader() => new ConsoleReader(this);

    /// <summary>
    /// 释放流资源。
    /// </summary>
    public void Dispose() => _stream.Dispose();

    /// <inheritdoc/>
    public void Dump(in DumpData data)
    {
        if (!CanDump())
        {
            return;
        }

        try
        {
            DumpResultObject(ResultObject.Create(data.Object, data.Quotas, data.Header, data.Line));
        }
        catch (Exception ex)
        {
            try
            {
                DumpMessage("Error during Dump: " + ex.Message);
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <inheritdoc/>
    public void DumpException(Exception exception)
    {
        if (!CanDump())
        {
            return;
        }

        try
        {
            DumpExceptionResultObject(ExceptionResultObject.Create(exception));
        }
        catch (Exception ex)
        {
            try
            {
                DumpMessage("Error during Dump: " + ex.Message);
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <inheritdoc/>
    public void DumpProgress(ProgressResultObject result)
    {
        lock (_lock)
        {
            Write(s_progressResultHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                using var _ = jsonWriter.WriteObject();
                if (result.Progress != null)
                {
                    jsonWriter.WriteProperty("p", result.Progress.Value);
                }
            }

            WriteNewLine();
        }
    }

    /// <inheritdoc/>
    public void Flush() => _stream.Flush();

    /// <summary>
    /// 检查是否可以继续输出（防止输出次数超过限制）。
    /// </summary>
    /// <returns>如果可以继续输出则返回 true。</returns>
    private bool CanDump()
    {
        var currentCount = Interlocked.Increment(ref _dumpCount);
        if (currentCount >= MaxDumpsPerSession)
        {
            if (currentCount == MaxDumpsPerSession)
            {
                DumpMessage("<max results reached>");
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 输出简单的文本消息。
    /// </summary>
    /// <param name="message">消息内容。</param>
    private void DumpMessage(string message)
    {
        lock (_lock)
        {
            Write(s_resultObjectHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                using var _ = jsonWriter.WriteObject();
                jsonWriter.WriteProperty("v", message);
            }

            WriteNewLine();
        }
    }

    /// <summary>
    /// 发送输入读取请求到宿主进程。
    /// </summary>
    internal void DumpInputReadRequest()
    {
        try
        {
            lock (_lock)
            {
                Write(s_inputReadRequestHeader);
                WriteNewLine();
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 输出异常结果对象。
    /// </summary>
    /// <param name="result">异常结果对象。</param>
    private void DumpExceptionResultObject(ExceptionResultObject result)
    {
        lock (_lock)
        {
            Write(s_exceptionResultHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                using var _ = jsonWriter.WriteObject();
                jsonWriter.WriteProperty("m", result.Message);
                WriteResultObjectContent(jsonWriter, result);
            }

            WriteNewLine();
        }
    }

    /// <summary>
    /// 输出结果对象。
    /// </summary>
    /// <param name="result">结果对象。</param>
    private void DumpResultObject(ResultObject result)
    {
        lock (_lock)
        {
            Write(s_resultObjectHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                WriteResultObject(jsonWriter, result, isRoot: true);
            }

            WriteNewLine();
        }
    }

    /// <summary>
    /// 写入换行符。
    /// </summary>
    private void WriteNewLine() => Write(s_newLine);

    /// <summary>
    /// 写入结果对象（包括开始和结束标记）。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="result">结果对象。</param>
    /// <param name="isRoot">是否为根对象。</param>
    private void WriteResultObject(XmlDictionaryWriter jsonWriter, ResultObject result, bool isRoot)
    {
        using var _ = jsonWriter.WriteObject(name: isRoot ? "root" : "item");
        WriteResultObjectContent(jsonWriter, result);
    }

    /// <summary>
    /// 写入结果对象的内容（不包括开始和结束标记）。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="result">结果对象。</param>
    private void WriteResultObjectContent(XmlDictionaryWriter jsonWriter, ResultObject result)
    {
        jsonWriter.WriteProperty("t", result.Type);
        jsonWriter.WriteProperty("h", result.Header);
        if (result.LineNumber is int lineNumber)
        {
            jsonWriter.WriteProperty("l", lineNumber);
        }

        jsonWriter.WriteProperty("v", result.Value);
        jsonWriter.WriteProperty("x", result.IsExpanded);

        if (result.Children != null)
        {
            using var _ = jsonWriter.WriteArray("c");

            foreach (var child in result.Children)
            {
                WriteResultObject(jsonWriter, child, isRoot: false);
            }
        }
    }

    /// <summary>
    /// 将字节数组写入输出流。
    /// </summary>
    /// <param name="bytes">要写入的字节数组。</param>
    private void Write(byte[] bytes) => _stream.Write(bytes, 0, bytes.Length);
}
#endif
