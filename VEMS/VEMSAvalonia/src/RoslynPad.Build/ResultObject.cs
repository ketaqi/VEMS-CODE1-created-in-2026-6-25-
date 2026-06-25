using System.Text;
using System.Text.Json.Serialization;

namespace RoslynPad.Build;

/// <summary>
/// 定义结果对象的接口。
/// </summary>
public interface IResultObject
{
    /// <summary>
    /// 获取结果值的字符串表示。
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// 将结果写入 StringBuilder。
    /// </summary>
    /// <param name="builder">目标 StringBuilder。</param>
    void WriteTo(StringBuilder builder);
}

/// <summary>
/// 定义带行号信息的结果接口。
/// </summary>
public interface IResultWithLineNumber
{
    /// <summary>
    /// 获取行号。
    /// </summary>
    int? LineNumber { get; }

    /// <summary>
    /// 获取列号。
    /// </summary>
    int Column { get; }
}

/// <summary>
/// 表示执行结果对象，可包含嵌套的子结果。
/// </summary>
public class ResultObject : IResultObject, IResultWithLineNumber
{
    /// <summary>
    /// 获取或设置结果标题。
    /// </summary>
    [JsonPropertyName("h")]
    public string? Header { get; set; }

    /// <summary>
    /// 获取或设置行号。
    /// </summary>
    [JsonPropertyName("l")]
    public int? LineNumber { get; set; }

    /// <inheritdoc/>
    int IResultWithLineNumber.Column => 0;

    /// <summary>
    /// 获取或设置结果值。
    /// </summary>
    [JsonPropertyName("v")]
    public string? Value { get; set; }

    /// <summary>
    /// 获取或设置结果类型名称。
    /// </summary>
    [JsonPropertyName("t")]
    public string? Type { get; set; }

    /// <summary>
    /// 获取或设置子结果列表。
    /// </summary>
    [JsonPropertyName("c")]
    public List<ResultObject>? Children { get; set; }

    /// <summary>
    /// 获取是否有子结果。
    /// </summary>
    public bool HasChildren => Children?.Count > 0;

    /// <summary>
    /// 获取或设置是否展开显示。
    /// </summary>
    [JsonPropertyName("x")]
    public bool IsExpanded { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        var builder = new StringBuilder();
        BuildStringRecursive(builder, 0);
        return builder.ToString();
    }

    /// <inheritdoc/>
    public void WriteTo(StringBuilder builder)
    {
        BuildStringRecursive(builder, 0);
    }

    private void BuildStringRecursive(StringBuilder builder, int level)
    {
        for (var i = 0; i < level; i++)
        {
            builder.Append("  ");
        }
        builder.Append(Header);
        if (Header != null && Value != null)
        {
            builder.Append(" = ");
        }
        builder.Append(Value);
        builder.AppendLine();
        if (Children != null)
        {
            foreach (var child in Children)
            {
                child.BuildStringRecursive(builder, level + 1);
            }
        }
    }
}

/// <summary>
/// 表示异常结果对象。
/// </summary>
public class ExceptionResultObject : ResultObject
{
    /// <summary>
    /// 获取或设置异常消息。
    /// </summary>
    [JsonPropertyName("m")]
    public string? Message { get; set; }
}

/// <summary>
/// 表示输入读取请求。
/// </summary>
public class InputReadRequest
{
}

/// <summary>
/// 表示进度结果对象。
/// </summary>
public class ProgressResultObject
{
    /// <summary>
    /// 获取或设置进度值（0.0 到 1.0）。
    /// </summary>
    [JsonPropertyName("p")]
    public double? Progress { get; set; }
}

/// <summary>
/// 表示编译错误结果对象。
/// </summary>
public class CompilationErrorResultObject : IResultObject, IResultWithLineNumber
{
    /// <summary>
    /// 获取或设置错误代码。
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 获取或设置严重性（Error/Warning）。
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// 获取或设置行号（1-based）。
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// 获取或设置列号（1-based）。
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// 获取或设置错误消息。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 创建编译错误结果对象。
    /// </summary>
    /// <param name="severity">严重性。</param>
    /// <param name="errorCode">错误代码。</param>
    /// <param name="message">错误消息。</param>
    /// <param name="line">行号（0-based）。</param>
    /// <param name="column">列号（0-based）。</param>
    /// <returns>编译错误结果对象。</returns>
    public static CompilationErrorResultObject Create(string severity, string errorCode, string message, int line, int column) => new()
    {
        ErrorCode = errorCode,
        Severity = severity,
        Message = message,
        // 0 to 1-based
        LineNumber = line + 1,
        Column = column + 1,
    };

    /// <inheritdoc/>
    public override string ToString() => $"{ErrorCode}: {Message}";

    /// <inheritdoc/>
    string? IResultObject.Value => ToString();

    /// <inheritdoc/>
    public void WriteTo(StringBuilder builder) => builder.Append(ToString());
}

/// <summary>
/// 表示还原结果对象。
/// </summary>
/// <param name="message">消息内容。</param>
/// <param name="severity">严重性。</param>
/// <param name="value">显示值。</param>
public class RestoreResultObject(string message, string severity, string? value = null) : IResultObject
{
    private readonly string? _value = value;

    /// <summary>
    /// 获取或设置消息内容。
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// 获取或设置严重性。
    /// </summary>
    public string Severity { get; set; } = severity;

    /// <summary>
    /// 获取显示值。
    /// </summary>
    public string Value => _value ?? Message;

    /// <inheritdoc/>
    public void WriteTo(StringBuilder builder) => builder.Append(Value);
}
