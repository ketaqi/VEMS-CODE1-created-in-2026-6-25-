namespace RoslynPad.Editor;

/// <summary>
/// 表示属性行为选项的枚举，支持按位组合
/// </summary>
[Flags]
public enum PropertyOptions
{
    /// <summary>
    /// 无选项
    /// </summary>
    None,

    /// <summary>
    /// 属性影响渲染过程
    /// </summary>
    AffectsRender = 1,

    /// <summary>
    /// 属性影响排列过程
    /// </summary>
    AffectsArrange = 2,

    /// <summary>
    /// 属性影响测量过程
    /// </summary>
    AffectsMeasure = 4,

    /// <summary>
    /// 属性支持双向绑定
    /// </summary>
    BindsTwoWay = 8,

    /// <summary>
    /// 属性值可继承
    /// </summary>
    Inherits = 16,
}
