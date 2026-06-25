using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn;

/// <summary>
/// 文档创建参数记录，包含创建文档所需的所有配置信息
/// </summary>
/// <param name="SourceTextContainer">源文本容器，用于存储和管理文档的文本内容</param>
/// <param name="WorkingDirectory">文档的工作目录</param>
/// <param name="SourceCodeKind">源代码类型（如脚本、常规代码等）</param>
/// <param name="OnTextUpdated">文本更新时的回调方法（可选）</param>
/// <param name="Name">文档名称（可选）</param>
public record DocumentCreationArgs(
    SourceTextContainer SourceTextContainer,
    string WorkingDirectory,
    SourceCodeKind SourceCodeKind,
    Action<SourceText>? OnTextUpdated = null,
    string? Name = null);
