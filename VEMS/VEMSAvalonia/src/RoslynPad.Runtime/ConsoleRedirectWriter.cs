using System.Text;

namespace RoslynPad.Runtime;

/// <summary>
/// 将控制台输出重定向到 Dump 方法的文本写入器。
/// </summary>
/// <param name="dumper">JSON 控制台输出器。</param>
/// <param name="header">输出标题。</param>
internal class ConsoleRedirectWriter(JsonConsoleDumper dumper, string? header = null) : TextWriter
{
    private readonly JsonConsoleDumper _dumper = dumper;
    private readonly string? _header = header;

    /// <summary>
    /// 获取编码（UTF-8）。
    /// </summary>
    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// 写入字符串到输出。
    /// </summary>
    /// <param name="value">要写入的字符串。</param>
    public override void Write(string? value)
    {
        if (string.Equals(Environment.NewLine, value, StringComparison.Ordinal))
        {
            return;
        }

        Dump(value);
    }

    /// <summary>
    /// 写入字符数组的指定部分到输出。
    /// </summary>
    /// <param name="buffer">字符缓冲区。</param>
    /// <param name="index">起始索引。</param>
    /// <param name="count">字符数量。</param>
    public override void Write(char[] buffer, int index, int count)
    {
        if (buffer != null)
        {
            if (EndsWithNewLine(buffer, index, count))
            {
                count -= Environment.NewLine.Length;
            }

            if (count > 0)
            {
                Dump(new string(buffer, index, count));
            }
        }
    }

    /// <summary>
    /// 检查缓冲区是否以换行符结尾。
    /// </summary>
    private bool EndsWithNewLine(char[] buffer, int index, int count)
    {
        var nl = Environment.NewLine;

        if (count < nl.Length)
        {
            return false;
        }

        for (int i = nl.Length; i >= 1; --i)
        {
            if (buffer[index + count - i] != nl[nl.Length - i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 写入单个字符到输出。
    /// </summary>
    /// <param name="value">要写入的字符。</param>
    public override void Write(char value) => Dump(value);

    /// <summary>
    /// 将值输出到控制台。
    /// </summary>
    private void Dump(object? value) => _dumper.Dump(new DumpData(value, _header, Line: null, DumpQuotas.Default));
}
