using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.QuickInfo;

/// <summary>
/// 快速信息项实体，包含文本范围和内容创建工厂
/// </summary>
public sealed class QuickInfoItem
{
    /// <summary>
    /// 内容创建工厂委托
    /// </summary>
    private readonly Func<object> _contentFactory;

    /// <summary>
    /// 获取快速信息对应的文本范围
    /// </summary>
    public TextSpan TextSpan { get; }

    /// <summary>
    /// 创建快速信息内容对象
    /// </summary>
    /// <returns>快速信息内容对象</returns>
    public object Create() => _contentFactory();

    /// <summary>
    /// 初始化 <see cref="QuickInfoItem"/> 实例
    /// </summary>
    /// <param name="textSpan">文本范围</param>
    /// <param name="contentFactory">内容创建工厂委托</param>
    internal QuickInfoItem(TextSpan textSpan, Func<object> contentFactory)
    {
        TextSpan = textSpan;
        _contentFactory = contentFactory;
    }
}
