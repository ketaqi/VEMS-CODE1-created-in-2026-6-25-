namespace RoslynPad.Runtime;

/// <summary>
/// 直接输出到控制台的 Dumper 实现，用于非 JSON 模式。
/// </summary>
internal class DirectConsoleDumper : IConsoleDumper
{
    private readonly object _lock = new();

    /// <summary>
    /// 获取是否支持重定向（不支持）。
    /// </summary>
    public bool SupportsRedirect => false;

    /// <summary>
    /// 创建写入器（不支持）。
    /// </summary>
    /// <exception cref="NotSupportedException">此方法不受支持。</exception>
    public TextWriter CreateWriter(string? header = null)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// 输出对象到控制台。
    /// </summary>
    /// <param name="data">要输出的数据。</param>
    public void Dump(in DumpData data)
    {
        try
        {
            DumpResultObject(ResultObject.Create(data.Object, data.Quotas, data.Header, data.Line));
        }
        catch (Exception ex)
        {
            try
            {
                Console.WriteLine("Error during Dump: " + ex.Message);
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// 输出异常（不支持）。
    /// </summary>
    /// <exception cref="NotSupportedException">此方法不受支持。</exception>
    public void DumpException(Exception exception) => throw new NotSupportedException();

    /// <summary>
    /// 创建读取器（不支持）。
    /// </summary>
    /// <exception cref="NotSupportedException">此方法不受支持。</exception>
    public TextReader CreateReader() => throw new NotSupportedException();

    /// <summary>
    /// 递归输出结果对象到控制台。
    /// </summary>
    private void DumpResultObject(ResultObject resultObject, int indent = 0)
    {
        lock (_lock)
        {
            if (indent > 0)
            {
                Console.Write("".PadLeft(indent));
            }

            Console.Write(resultObject.HasChildren ? "+ " : "  ");

            if (resultObject.Header != null)
            {
                Console.Write($"[{resultObject.Header}]: ");
            }

            Console.WriteLine(resultObject.Value);

            if (resultObject.Children != null)
            {
                foreach (var child in resultObject.Children)
                {
                    DumpResultObject(child, indent + 2);
                }
            }

            if (indent == 0)
            {
                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// 刷新输出（无操作）。
    /// </summary>
    public void Flush()
    {
    }

    /// <summary>
    /// 输出进度（无操作）。
    /// </summary>
    public void DumpProgress(ProgressResultObject result)
    {
    }
}
