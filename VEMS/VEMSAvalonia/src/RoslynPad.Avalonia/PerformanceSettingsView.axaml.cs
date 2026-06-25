using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RoslynPad.UI;
using Avalonia.VisualTree;

namespace RoslynPad;

/// <summary>
/// 文档树视图（第 2 区域）：
/// 处理树节点的双击与 Enter 打开文档；
/// 以及“路径输入框”回车切换目录、Esc 清空输入的快捷行为。
/// </summary>
/// <remarks>
/// - 打开文档：从事件源向上溯源最近的 <see cref="TreeViewItem"/>，读取其 <c>DataContext</c>
///   为 <see cref="DocumentViewModel"/>，并调用 <see cref="MainViewModel.OpenDocument(DocumentViewModel)"/>。<br/>
/// - 切换目录：在路径输入框里回车时，如果路径有效，则调用 <see cref="MainViewModel.OpenFolderByPath1(string)"/>。<br/>
/// 线程模型：仅 UI 线程事件处理，不做耗时操作。
/// </remarks>
public partial class PerformanceSettingsView : UserControl
{
    /// <summary>当前页面绑定的主视图模型。</summary>
    private MainViewModel? _viewModel;

    /// <summary>
    /// 构造函数：初始化组件并为树控件注册双击与键盘事件。
    /// </summary>
    public PerformanceSettingsView()
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
    /// DataContext 变更时缓存 <see cref="MainViewModel"/> 以便后续打开文档/目录调用。
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
    /// 读取其 <c>DataContext</c> 为 <see cref="DocumentViewModel"/>，然后调用主 VM 打开。
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

    /// <summary>
    /// 路径输入框键盘事件：回车尝试切换目录；Esc 清空输入。
    /// </summary>
    /// <param name="sender">绑定到输入框的控件（通常是 <see cref="TextBox"/>）。</param>
    /// <param name="e">键盘事件参数。</param>
    /// <remarks>
    /// - Enter：若输入的本地路径存在，则调用 <see cref="MainViewModel.OpenFolderByPath1(string)"/> 切换目录；否则在控制台输出提示。<br/>
    /// - Escape：清空文本框内容。<br/>
    /// 注：此方法只负责 UI 行为与简单日志输出；如需提示到 UI，请在外部补充消息机制。
    /// </remarks>
    private void PathInputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        // 按下回车时，尝试设置路径
        if (e.Key == Key.Enter)
        {
            var inputPath = tb.Text?.Trim();
            if (!string.IsNullOrEmpty(inputPath))
            {
                if (System.IO.Directory.Exists(inputPath))
                {
                    _viewModel?.OpenFolderByPath1(inputPath); // 调用 VM 切换目录
                    Console.WriteLine($"[路径输入] 已切换到: {inputPath}");
                }
                else
                {
                    Console.WriteLine($"[路径输入] 路径无效或不存在: {inputPath}");
                }
            }
            else
            {
                Console.WriteLine("[路径输入] 请输入有效路径");
            }
            e.Handled = true; // 防止回车继续向上冒泡或触发默认行为
            return;
        }

        // 按下 Esc 清空输入
        if (e.Key == Key.Escape)
        {
            tb.Text = string.Empty;
            Console.WriteLine("[路径输入] 已清空输入框");
            e.Handled = true;
            return;
        }
    }
}
