namespace RoslynPad.Editor;

/// <summary>
/// 代码文本编辑器控件，继承自 Avalonia 文本编辑器基类
/// 提供代码编辑相关的鼠标悬停、提示框等扩展功能
/// </summary>
public partial class CodeTextEditor
{
    /// <summary>
    /// 重写样式键，指定当前控件使用 TextEditor 类型的样式
    /// </summary>
    protected override Type StyleKeyOverride => typeof(TextEditor);

    /// <summary>
    /// 初始化控件事件
    /// 绑定鼠标悬停和悬停停止事件
    /// </summary>
    partial void Initialize()
    {
        PointerHover += OnMouseHover;
        PointerHoverStopped += OnMouseHoverStopped;
    }

    /// <summary>
    /// 初始化工具提示（Tooltip）配置
    /// 设置提示框显示延迟、内容，并监听提示框关闭事件清理资源
    /// </summary>
    partial void InitializeToolTip()
    {
        if (_toolTip == null)
        {
            return;
        }

        // 设置提示框显示延迟为 0 毫秒
        ToolTip.SetShowDelay(this, 0);
        // 绑定提示框到当前控件
        ToolTip.SetTip(this, _toolTip);
        // 监听提示框是否打开的属性变化，关闭时清空提示框引用
        _toolTip.GetPropertyChangedObservable(ToolTip.IsOpenProperty).Subscribe(c =>
        {
            if (c.NewValue as bool? != true)
            {
                _toolTip = null;
            }
        });
    }

    /// <summary>
    /// 提示框打开后的后续处理
    /// 强制刷新提示框的视觉呈现
    /// </summary>
    partial void AfterToolTipOpen()
    {
        _toolTip?.InvalidateVisual();
    }
}
