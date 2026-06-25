using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RoslynPad.Roslyn.Resources;

/// <summary>
/// 字形资源字典类，用于管理RoslynPad中与字形相关的Avalonia资源
/// </summary>
public class Glyphs : ResourceDictionary
{
    /// <summary>
    /// 初始化Glyphs类的新实例
    /// </summary>
    public Glyphs()
    {
        // 加载关联的AXAML资源文件到当前资源字典
        AvaloniaXamlLoader.Load(this);
    }
}
