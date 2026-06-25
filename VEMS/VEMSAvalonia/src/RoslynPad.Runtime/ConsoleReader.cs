namespace RoslynPad.Runtime;

/// <summary>
/// 支持 JSON 通信协议的控制台输入读取器。
/// </summary>
/// <param name="dumper">JSON 控制台输出器，用于发送输入请求。</param>
internal class ConsoleReader(JsonConsoleDumper dumper) : TextReader
{
    private readonly TextReader _reader = new StreamReader(Console.OpenStandardInput());
    private readonly JsonConsoleDumper _dumper = dumper;

    private string? _readString;
    private int _readPosition;

    /// <summary>
    /// 读取下一个字符。如果缓冲区为空，会发送输入请求并等待用户输入。
    /// </summary>
    /// <returns>读取的字符，或在流结尾时返回 -1。</returns>
    public override int Read()
    {
        if (_readString == null || _readPosition >= _readString.Length - 1)
        {
            _dumper.DumpInputReadRequest();

            _readString = _reader.ReadLine() + Environment.NewLine;
            _readPosition = 0;
        }

        return _readString[_readPosition++];
    }

    /// <summary>
    /// 释放资源。
    /// </summary>
    /// <param name="disposing">是否释放托管资源。</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _reader.Dispose();
        }
    }
}
