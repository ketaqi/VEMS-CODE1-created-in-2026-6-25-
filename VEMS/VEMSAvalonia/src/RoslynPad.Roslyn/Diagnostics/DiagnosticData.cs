using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 诊断数据封装类，用于包装Roslyn原生诊断数据并提供统一访问接口，支持相等性比较
/// </summary>
public sealed class DiagnosticData : IEquatable<DiagnosticData>
{
    /// <summary>
    /// 内部封装的Roslyn原生诊断数据实例
    /// </summary>
    private readonly Microsoft.CodeAnalysis.Diagnostics.DiagnosticData _inner;

    /// <summary>
    /// 初始化<see cref="DiagnosticData"/>实例
    /// </summary>
    /// <param name="inner">Roslyn原生诊断数据实例</param>
    internal DiagnosticData(Microsoft.CodeAnalysis.Diagnostics.DiagnosticData inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// 获取诊断标识ID
    /// </summary>
    public string Id => _inner.Id;

    /// <summary>
    /// 获取诊断分类
    /// </summary>
    public string Category => _inner.Category;

    /// <summary>
    /// 获取诊断消息内容
    /// </summary>
    public string? Message => _inner.Message;

    /// <summary>
    /// 获取诊断描述信息
    /// </summary>
    public string? Description => _inner.Description;

    /// <summary>
    /// 获取诊断标题
    /// </summary>
    public string? Title => _inner.Title;

    /// <summary>
    /// 获取诊断帮助链接地址
    /// </summary>
    public string? HelpLink => _inner.HelpLink;

    /// <summary>
    /// 获取诊断级别
    /// </summary>
    public DiagnosticSeverity Severity => _inner.Severity;

    /// <summary>
    /// 获取诊断默认级别
    /// </summary>
    public DiagnosticSeverity DefaultSeverity => _inner.DefaultSeverity;

    /// <summary>
    /// 获取一个值，指示该诊断是否默认启用
    /// </summary>
    public bool IsEnabledByDefault => _inner.IsEnabledByDefault;

    /// <summary>
    /// 获取诊断警告级别
    /// </summary>
    public int WarningLevel => _inner.WarningLevel;

    /// <summary>
    /// 获取诊断的自定义标签列表
    /// </summary>
    public IReadOnlyList<string> CustomTags => _inner.CustomTags;

    /// <summary>
    /// 获取诊断的自定义属性字典
    /// </summary>
    public ImmutableDictionary<string, string?> Properties => _inner.Properties;

    /// <summary>
    /// 获取一个值，指示该诊断是否被抑制
    /// </summary>
    public bool IsSuppressed => _inner.IsSuppressed;

    /// <summary>
    /// 获取诊断所属的项目ID
    /// </summary>
    public ProjectId? ProjectId => _inner.ProjectId;

    /// <summary>
    /// 获取诊断所属的文档ID
    /// </summary>
    public DocumentId? DocumentId => _inner.DocumentId;

    /// <summary>
    /// 根据指定的源文本获取诊断对应的文本范围（已钳位）
    /// </summary>
    /// <param name="sourceText">源文本实例</param>
    /// <returns>诊断对应的文本范围，若不存在则为null</returns>
    public TextSpan? GetTextSpan(SourceText sourceText) => _inner.DataLocation.MappedFileSpan.GetClampedTextSpan(sourceText);

    /// <summary>
    /// 判断当前实例是否与另一个<see cref="DiagnosticData"/>实例相等
    /// </summary>
    /// <param name="other">要比较的另一个实例</param>
    /// <returns>相等则返回true，否则返回false</returns>
    public bool Equals(DiagnosticData? other) => _inner.Equals(other?._inner);

    /// <summary>
    /// 判断当前实例是否与指定对象相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>相等则返回true，否则返回false</returns>
    public override bool Equals(object? obj) => obj is DiagnosticData other && Equals(other);

    /// <summary>
    /// 获取当前实例的哈希码
    /// </summary>
    /// <returns>哈希码值</returns>
    public override int GetHashCode() => _inner.GetHashCode();
}
