namespace RoslynPad.Editor;

/// <summary>
/// 定义分类高亮颜色的提供接口
/// </summary>
public interface IClassificationHighlightColors
{
    /// <summary>
    /// 获取默认的高亮颜色
    /// </summary>
    HighlightingColor DefaultBrush { get; }

    /// <summary>
    /// 根据分类类型名称获取对应的高亮颜色
    /// </summary>
    /// <param name="classificationTypeName">分类类型名称</param>
    /// <returns>对应的高亮颜色实例</returns>
    HighlightingColor GetBrush(string classificationTypeName);
}
