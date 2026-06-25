using Avalonia.Data;

namespace RoslynPad.Editor;

/// <summary>
/// 上下文操作灯泡（智能提示）的上下文菜单控件
/// </summary>
internal class ContextActionsBulbContextMenu : MenuFlyout
{
    private readonly ActionCommandConverter _converter;

    /// <summary>
    /// 初始化<see cref="ContextActionsBulbContextMenu"/>类的新实例
    /// </summary>
    /// <param name="converter">命令转换工具实例</param>
    public ContextActionsBulbContextMenu(ActionCommandConverter converter)
    {
        _converter = converter;
        // 设置菜单弹出位置为右侧
        Placement = PlacementMode.Right;
    }

    /// <summary>
    /// 创建菜单项容器样式（绑定命令转换器）
    /// </summary>
    /// <returns>菜单项样式实例</returns>
    private Style CreateItemContainerStyle() => new(s => s.OfType<MenuItem>())
    {
        Setters =
        {
            new Setter(MenuItem.CommandProperty, new Binding { Converter = _converter })
        }
    };

    /// <summary>
    /// 创建菜单展示器（重写基类方法，添加自定义样式）
    /// </summary>
    /// <returns>配置后的菜单展示器</returns>
    protected override Control CreatePresenter()
    {
        var presenter = base.CreatePresenter();
        presenter.Styles.Add(CreateItemContainerStyle());
        return presenter;
    }
}
