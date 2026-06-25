using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Classification;
using RoslynPad.Roslyn.Classification;

namespace RoslynPad.Editor;

/// <summary>
/// 代码分类高亮颜色配置类，定义各类代码元素的高亮颜色
/// </summary>
public class ClassificationHighlightColors : IClassificationHighlightColors
{
    /// <summary>
    /// 默认高亮颜色（黑色前景）
    /// </summary>
    public HighlightingColor DefaultBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Black) };

    /// <summary>
    /// 类型（类、结构体等）高亮颜色（蓝绿色前景）
    /// </summary>
    public HighlightingColor TypeBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Teal) };

    /// <summary>
    /// 方法高亮颜色（橄榄色前景）
    /// </summary>
    public HighlightingColor MethodBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Olive) };

    /// <summary>
    /// 参数高亮颜色（深蓝色前景）
    /// </summary>
    public HighlightingColor ParameterBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.DarkBlue) };

    /// <summary>
    /// 注释高亮颜色（绿色前景）
    /// </summary>
    public HighlightingColor CommentBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Green) };

    /// <summary>
    /// XML注释高亮颜色（灰色前景）
    /// </summary>
    public HighlightingColor XmlCommentBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Gray) };

    /// <summary>
    /// 关键字高亮颜色（蓝色前景）
    /// </summary>
    public HighlightingColor KeywordBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Blue) };

    /// <summary>
    /// 预处理关键字高亮颜色（灰色前景）
    /// </summary>
    public HighlightingColor PreprocessorKeywordBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Gray) };

    /// <summary>
    /// 字符串高亮颜色（褐红色前景）
    /// </summary>
    public HighlightingColor StringBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Maroon) };

    /// <summary>
    /// 括号匹配高亮颜色（黑色前景、浅黄绿色背景）
    /// </summary>
    public HighlightingColor BraceMatchingBrush { get; protected set; } = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Black), Background = new SimpleHighlightingBrush(Color.FromArgb(150, 219, 224, 204)) };

    /// <summary>
    /// 静态符号高亮颜色（粗体）
    /// </summary>
    public HighlightingColor StaticSymbolBrush { get; protected set; } = new HighlightingColor { FontWeight = FontWeights.Bold };

    /// <summary>
    /// 延迟初始化的分类类型与高亮颜色映射字典
    /// </summary>
    private readonly Lazy<ImmutableDictionary<string, HighlightingColor>> _map;

    /// <summary>
    /// 初始化<ClassificationHighlightColors>实例，构建分类颜色映射
    /// </summary>
    public ClassificationHighlightColors()
    {
        _map = new Lazy<ImmutableDictionary<string, HighlightingColor>>(() => new Dictionary<string, HighlightingColor>
        {
            [ClassificationTypeNames.ClassName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.RecordClassName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.RecordStructName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.StructName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.InterfaceName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.DelegateName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.EnumName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.ModuleName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.TypeParameterName] = TypeBrush.AsFrozen(),
            [ClassificationTypeNames.MethodName] = MethodBrush.AsFrozen(),
            [ClassificationTypeNames.ExtensionMethodName] = MethodBrush.AsFrozen(),
            [ClassificationTypeNames.ParameterName] = ParameterBrush.AsFrozen(),
            [ClassificationTypeNames.Comment] = CommentBrush.AsFrozen(),
            [ClassificationTypeNames.StaticSymbol] = StaticSymbolBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentAttributeName] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentAttributeQuotes] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentAttributeValue] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentCDataSection] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentComment] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentDelimiter] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentEntityReference] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentName] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentProcessingInstruction] = XmlCommentBrush.AsFrozen(),
            [ClassificationTypeNames.XmlDocCommentText] = CommentBrush.AsFrozen(),
            [ClassificationTypeNames.Keyword] = KeywordBrush.AsFrozen(),
            [ClassificationTypeNames.ControlKeyword] = KeywordBrush.AsFrozen(),
            [ClassificationTypeNames.PreprocessorKeyword] = PreprocessorKeywordBrush.AsFrozen(),
            [ClassificationTypeNames.StringLiteral] = StringBrush.AsFrozen(),
            [ClassificationTypeNames.VerbatimStringLiteral] = StringBrush.AsFrozen(),
            [AdditionalClassificationTypeNames.BraceMatching] = BraceMatchingBrush.AsFrozen()
        }.ToImmutableDictionary());
    }

    /// <summary>
    /// 获取或创建分类类型与高亮颜色的映射字典（可被子类重写）
    /// </summary>
    /// <returns>不可变的映射字典</returns>
    protected virtual ImmutableDictionary<string, HighlightingColor> GetOrCreateMap()
    {
        return _map.Value;
    }

    /// <summary>
    /// 根据分类类型名称获取对应的高亮颜色
    /// </summary>
    /// <param name="classificationTypeName">分类类型名称</param>
    /// <returns>对应的高亮颜色，若未找到则返回默认颜色</returns>
    public HighlightingColor GetBrush(string classificationTypeName)
    {
        GetOrCreateMap().TryGetValue(classificationTypeName, out var brush);
        return brush ?? DefaultBrush.AsFrozen();
    }
}
