using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RoslynPad.UI;
using Avalonia.VisualTree;

namespace RoslynPad;

/// <summary>
/// 文档树视图（第 5 区域）：
/// 负责处理树节点的双击与键盘 Enter 打开文档的交互，
/// 并把选中的 <see cref="DocumentViewModel"/> 转交给 <see cref="MainViewModel.OpenDocument(DocumentViewModel)"/>。
/// </summary>
/// <remarks>
/// 事件来源可能是任意子元素（图标/文本等），通过从 <see cref="RoutedEventArgs.Source"/> 向上溯源到最近的
/// <see cref="TreeViewItem"/> 来取得其 <c>DataContext</c>。  
/// 线程模型：仅进行 UI 线程上的事件处理与调用，不做耗时操作。
/// </remarks>
public partial class RunAndDebugView : UserControl
{
    /// <summary>当前页面绑定的主视图模型。</summary>
    private MainViewModel? _viewModel;

    /// <summary>
    /// 构造函数：初始化组件并为树控件注册双击与键盘事件。
    /// </summary>
    public RunAndDebugView()
    {
        InitializeComponent();

        // 从模板中查找名为 "Tree" 的 TreeView，并挂接交互事件
        var treeView = this.Find<TreeView>("Tree");
        if (treeView != null)
        {
            treeView.DoubleTapped += OnDocumentClick;  // 双击打开
            treeView.KeyDown += OnDocumentKeyDown;     // Enter 打开
        }
    }

    /// <summary>
    /// DataContext 变更时缓存 <see cref="MainViewModel"/> 以供后续打开文档调用。
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext != null)
            _viewModel = DataContext as MainViewModel;
    }

    /// <summary>
    /// 双击节点时触发：尝试打开对应文档。
    /// </summary>
    private void OnDocumentClick(object? sender, RoutedEventArgs e)
    {
        OpenDocument(e.Source);
    }

    /// <summary>
    /// 键盘处理：在树上按下 Enter 键时尝试打开对应文档。
    /// </summary>
    private void OnDocumentKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OpenDocument(e.Source);
        }
    }

    /// <summary>
    /// 执行打开文档的核心逻辑：从事件源向上找到最近的 <see cref="TreeViewItem"/>，
    /// 读取其 <c>DataContext</c> 为 <see cref="DocumentViewModel"/>，然后调用 <see cref="MainViewModel.OpenDocument(DocumentViewModel)"/>。
    /// </summary>
    /// <param name="source">事件源对象（可能是图标、文本或 <see cref="TreeViewItem"/> 本身）。</param>
    private void OpenDocument(object? source)
    {
        // 事件源不一定就是 TreeViewItem：沿视觉树向上查找最近的 TreeViewItem
        var item = (source as Visual)?.GetSelfAndVisualAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();

        // 若该项绑定的是 DocumentViewModel，则交由主 VM 打开
        if (item?.DataContext is DocumentViewModel documentViewModel)
        {
            _viewModel?.OpenDocument(documentViewModel);
        }
    }
}
