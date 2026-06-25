#if NET6_0_OR_GREATER
using System.Text;
using System.Text.Json;

namespace RoslynPad.Runtime;

/// <summary>
/// 使用 System.Text.Json 的 JSON 控制台输出器（.NET 6+）。
/// </summary>
internal class JsonConsoleDumper : IConsoleDumper, IDisposable
{
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

    private Utf8JsonWriter CreateJsonWriter() => new(_stream);

    /// <inheritdoc/>
    public bool SupportsRedirect => true;

    /// <inheritdoc/>
    public TextWriter CreateWriter(string? header = null) => new ConsoleRedirectWriter(this, header);

    /// <inheritdoc/>
    public TextReader CreateReader() => new ConsoleReader(this);

    /// <summary>
    /// 释放资源。
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
                jsonWriter.WriteStartObject();

                if (result.Progress != null)
                {
                    jsonWriter.WriteNumber("p", result.Progress.Value);
                }

                jsonWriter.WriteEndObject();
            }

            WriteNewLine();
        }
    }

    /// <inheritdoc/>
    public void Flush() => _stream.Flush();

    /// <summary>
    /// 检查是否可以继续输出（防止输出过多）。
    /// </summary>
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
    /// 输出简单消息。
    /// </summary>
    private void DumpMessage(string message)
    {
        lock (_lock)
        {
            Write(s_resultObjectHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("v", message);
                jsonWriter.WriteEndObject();
            }

            WriteNewLine();
        }
    }

    /// <summary>
    /// 输出输入读取请求。
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
    private void DumpExceptionResultObject(ExceptionResultObject result)
    {
        lock (_lock)
        {
            Write(s_exceptionResultHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                jsonWriter.WriteStartObject();
                try
                {
                    jsonWriter.WriteString("m", result.Message);
                    WriteResultObjectContent(jsonWriter, result);
                }
                finally
                {
                    jsonWriter.WriteEndObject();
                }
            }

            WriteNewLine();
        }
    }

    /// <summary>
    /// 输出结果对象。
    /// </summary>
    private void DumpResultObject(ResultObject result)
    {
        lock (_lock)
        {
            Write(s_resultObjectHeader);

            using (var jsonWriter = CreateJsonWriter())
            {
                WriteResultObject(jsonWriter, result);
            }

            WriteNewLine();
        }
    }

    private void WriteNewLine() => Write(s_newLine);

    private void WriteResultObject(Utf8JsonWriter jsonWriter, ResultObject result)
    {
        jsonWriter.WriteStartObject();
        try
        {
            WriteResultObjectContent(jsonWriter, result);
        }
        finally
        {
            jsonWriter.WriteEndObject();
        }
    }

    private void WriteResultObjectContent(Utf8JsonWriter jsonWriter, ResultObject result)
    {
        jsonWriter.WriteString("t", result.Type);
        jsonWriter.WriteString("h", result.Header);
        if (result.LineNumber is int lineNumber)
        {
            jsonWriter.WriteNumber("l", lineNumber);
        }

        jsonWriter.WriteString("v", result.Value);
        jsonWriter.WriteBoolean("x", result.IsExpanded);

        if (result.Children != null)
        {
            jsonWriter.WriteStartArray("c");

            try
            {
                foreach (var child in result.Children)
                {
                    WriteResultObject(jsonWriter, child);
                }
            }
            finally
            {
                jsonWriter.WriteEndArray();
            }
        }
    }

    private void Write(byte[] bytes) => _stream.Write(bytes, 0, bytes.Length);
}
#endif
