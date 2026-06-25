using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RoslynPad.UI;
using RoslynPad.ViewModels;

namespace RoslynPad;

/// <summary>
/// 文档树视图：
/// - 处理树节点的双击/Enter 打开文档；
/// - 右键菜单“打开/展开/折叠/内联重命名”；
/// - 选择变化同步到 <see cref="MainViewModel.SelectedDocument"/>；
/// - 支持“右键点击仅选中但不触发预览”的抑制逻辑；
/// - 内联重命名时自动定位并聚焦到重命名文本框；
/// - 快捷键输入框按键录入（可追加“Ctrl+K, Ctrl+C”这类组合）。
/// </summary>
/// <remarks>
/// 事件源可能是任意子元素（图标/文本等），通过从 <see cref="RoutedEventArgs.Source"/> 沿视觉树向上查找
/// 最近的 <see cref="TreeViewItem"/> 来获取其 <c>DataContext</c>（<see cref="DocumentViewModel"/>）。<br/>
/// 线程模型：仅进行 UI 线程上的事件处理与调用，不做耗时操作。
/// </remarks>
public partial class DocumentTreeView : UserControl
{
    /// <summary>页面绑定的主视图模型。</summary>
    private MainViewModel? _viewModel;

    /// <summary>
    /// 右键抑制预览标记：当为右键引发的选中变化时，不触发“选中即预览”的联动。
    /// </summary>
    private bool _suppressPreviewByRightClick;

    /// <summary>
    /// 构造函数：初始化组件，注册树的交互事件与顶级指针按下事件（用于识别右键）。
    /// </summary>
    public DocumentTreeView()
    {
        InitializeComponent();

        // 查找模板中的 TreeView 并挂接常用事件
        var treeView = this.Find<TreeView>("Tree");
        if (treeView != null)
        {
            treeView.DoubleTapped += OnDocumentClick;       // 双击打开
            treeView.KeyDown += OnDocumentKeyDown;          // Enter 打开
            treeView.SelectionChanged += OnTreeViewSelectionChanged; // 选中变化
        }

        // 监听指针按下（Tunnel）以记录是否为右键，后续用于抑制预览
        this.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// 指针按下：记录当前是否由右键触发（不设置 <see cref="RoutedEventArgs.Handled"/>，避免影响右键菜单弹出）。
    /// </summary>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pp = e.GetCurrentPoint(this);
        _suppressPreviewByRightClick = pp.Properties.IsRightButtonPressed;
        // 注意：不置 handled，避免拦截右键菜单
    }

    /// <summary>
    /// DataContext 变化时，退订旧 VM 的通知并订阅新 VM 的通知（用于选中项触发内联重命名时聚焦文本框）。
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        var old = _viewModel;
        if (old is INotifyPropertyChanged inpcOld)
            inpcOld.PropertyChanged -= ViewModelOnPropertyChanged;

        _viewModel = DataContext as MainViewModel;

        if (_viewModel is INotifyPropertyChanged inpcNew)
            inpcNew.PropertyChanged += ViewModelOnPropertyChanged;
    }

    /// <summary>
    /// 监听 VM 属性变化：当 <see cref="MainViewModel.SelectedDocument"/> 切换为“正在编辑名称”的项时，聚焦到内联重命名框。
    /// </summary>
    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedDocument))
        {
            var vm = _viewModel?.SelectedDocument as DocumentViewModel;
            if (vm is not null && vm.IsEditing)
            {
                _ = FocusInlineRenameAsync(vm);
            }
        }
    }

    /// <summary>
    /// 异步聚焦到内联重命名文本框：展开祖先→选中该项→等待一帧渲染→查找并聚焦名称为 "InlineRenameBox" 的 <see cref="TextBox"/>。
    /// </summary>
    /// <param name="vm">目标文档项。</param>
    private Task FocusInlineRenameAsync(DocumentViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        // 返回 UI 调度的 Task（不在此处 await，避免编译器警告）
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var tree = this.Find<TreeView>("Tree");
            if (tree is null) return;

            // 1) 展开所有祖先节点，确保容器被生成
            for (var p = vm.Parent; p is not null; p = p.Parent)
                p.IsExpanded = true;

            // 2) 选中该项（触发容器/模板创建）
            tree.SelectedItem = vm;

            // 3) 等一帧渲染，给模板时间
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            // 4) 重试查找重命名文本框
            const int MaxTries = 5;
            TextBox? tb = null;
            for (int attempt = 0; attempt < MaxTries && tb is null; attempt++)
            {
                var tvi = tree.GetVisualDescendants()
                              .OfType<TreeViewItem>()
                              .FirstOrDefault(i => ReferenceEquals(i.DataContext, vm));
                if (tvi is not null)
                {
                    tvi.BringIntoView(); // 虚拟化场景下很关键
                    tb = tvi.GetVisualDescendants()
                            .OfType<TextBox>()
                            .FirstOrDefault(x => x.Name == "InlineRenameBox");
                }

                if (tb is null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
                }
            }

            if (tb is not null)
            {
                tb.Focus();
                tb.SelectAll();
            }
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// 双击树节点：尝试打开对应文档或切换目录。
    /// </summary>
    private void OnDocumentClick(object? sender, RoutedEventArgs e)
    {
        OpenDocument(e.Source);
    }

    /// <summary>
    /// 树上按 Enter：尝试打开对应文档或切换目录。
    /// </summary>
    private void OnDocumentKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OpenDocument(e.Source);
        }
    }

    /// <summary>
    /// 核心：从事件源沿视觉树查找最近的 <see cref="TreeViewItem"/>，读取其 <c>DataContext</c> 为
    /// <see cref="DocumentViewModel"/>，随后调用 <see cref="MainViewModel.OpenDocument(DocumentViewModel)"/>。
    /// </summary>
    private void OpenDocument(object? source)
    {
        Console.WriteLine("[OpenDocument] 方法被调用，source类型: " + (source?.GetType().Name ?? "null"));

        var item = (source as Visual)?.GetSelfAndVisualAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();

        if (item == null)
        {
            Console.WriteLine("[OpenDocument] 未找到 TreeViewItem，source 可能不是有效的 Visual 或未在树中。");
        }
        else
        {
            Console.WriteLine("[OpenDocument] 找到 TreeViewItem，DataContext类型: " + (item.DataContext?.GetType().Name ?? "null"));
        }

        if (item?.DataContext is DocumentViewModel documentViewModel)
        {
            Console.WriteLine($"[OpenDocument] 打开项: {(documentViewModel.IsFolder ? "文件夹" : "文件")}, 名称: {documentViewModel.Name}, 路径: {documentViewModel.Path}");
            _viewModel?.OpenDocument(documentViewModel);
        }
        else
        {
            Console.WriteLine("[OpenDocument] 未能获取到 DocumentViewModel，无法打开文件或文件夹");
        }
    }

    /// <summary>
    /// 右键菜单“打开”项：若为文件夹则切换 <c>IsExpanded</c> 展开/折叠；否则打开文件。
    /// </summary>
    private void MenuItem_Open_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            // 优先采用 ContextMenu.PlacementTarget
            var contextMenu = menuItem.Parent as ContextMenu;
            var dockPanel = contextMenu?.PlacementTarget as DockPanel;

            // 回退：通过视觉树查找 DockPanel
            if (dockPanel == null)
            {
                dockPanel = menuItem.GetVisualAncestors().OfType<DockPanel>().FirstOrDefault();
                Console.WriteLine("[Debug] DockPanel was null, found via visual tree: " + (dockPanel != null));
            }

            if (dockPanel != null)
            {
                Console.WriteLine($"[Debug] DockPanel.DataContext type: {dockPanel.DataContext?.GetType().FullName}");
                Console.WriteLine($"[Debug] DockPanel.DataContext value: {dockPanel.DataContext}");
            }
            else
            {
                Console.WriteLine("[Debug] DockPanel is still null after visual tree search");
            }

            if (dockPanel?.DataContext is DocumentViewModel documentViewModel)
            {
                if (documentViewModel.IsFolder)
                {
                    documentViewModel.IsExpanded = !documentViewModel.IsExpanded;
                    Console.WriteLine($"[DocumentTreeView] 文件夹已{(documentViewModel.IsExpanded ? "展开" : "折叠")}: {documentViewModel.Name}");
                }
                else
                {
                    _viewModel?.OpenDocument(documentViewModel);
                    Console.WriteLine($"[DocumentTreeView] 打开文件: Name={documentViewModel.Name}, Path={documentViewModel.Path}");
                }
            }
            else
            {
                Console.WriteLine("[DocumentTreeView] MenuItem_Open_Click failed: DataContext is not DocumentViewModel");
            }
        }
    }

    /// <summary>
    /// 右键菜单“打开”（备用版本）：逻辑同上，保留调试日志以便问题定位。
    /// </summary>
    private void MenuItem_Open_Click1(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("[DEBUG] MenuItem_Open_Click1 事件已触发");

        string eventName = e.RoutedEvent?.Name ?? "null";
        Debug.WriteLine($"事件类型：{eventName}，触发时间：{DateTime.Now:HH:mm:ss.fff}");

        if (sender is MenuItem menuItem)
        {
            if (menuItem.DataContext is DocumentViewModel documentViewModel)
            {
                Console.WriteLine($"[TEST] 当前选中项类型: {(documentViewModel.IsFolder ? "文件夹" : "文件")}, 名称: {documentViewModel.Name}, 路径: {documentViewModel.Path}");
                if (documentViewModel.IsFolder)
                {
                    Console.WriteLine($"[MenuItem_Open_Click1] 切换前 IsExpanded: {documentViewModel.IsExpanded}");
                    documentViewModel.IsExpanded = !documentViewModel.IsExpanded;
                    Console.WriteLine($"[MenuItem_Open_Click1] 切换后 IsExpanded: {documentViewModel.IsExpanded}");
                    Console.WriteLine($"[MenuItem_Open_Click1] 文件夹已{(documentViewModel.IsExpanded ? "展开" : "折叠")}: {documentViewModel.Name}");
                }
                else
                {
                    _viewModel?.OpenDocument(documentViewModel);
                }
            }
            else
            {
                Console.WriteLine("[WARNING] MenuItem.DataContext 不是 DocumentViewModel，无法打开文件或文件夹");
            }
        }
        else
        {
            Console.WriteLine("[WARNING] sender 不是 MenuItem，无法获取 DataContext");
        }
    }

    /// <summary>
    /// 右键菜单“重命名”项：进入内联编辑模式（切换到文本框）。
    /// </summary>
    private void OnInlineRenameMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is DocumentViewModel vm)
        {
            vm.IsEditing = true;
        }
    }

    //private void OnInlineRenameDoubleTapped(object sender, RoutedEventArgs e)
    //{
    //    if (sender is TextBlock tb && tb.DataContext is DocumentViewModel vm)
    //    {
    //        vm.IsEditing = true;
    //    }
    //}

    /// <summary>
    /// 重命名输入框失去焦点：结束编辑并提交重命名。
    /// </summary>
    private void OnInlineRenameLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is DocumentViewModel vm)
        {
            vm.IsEditing = false;
            RenameDocument(vm, tb.Text ?? string.Empty); // 保证 newName 非 null
        }

    }

    /// <summary>
    /// 重命名输入框按键：Enter 提交；Esc 取消（不提交）。
    /// </summary>
    private void OnInlineRenameKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is DocumentViewModel vm)
        {
            if (e.Key == Key.Enter)
            {
                vm.IsEditing = false;
                RenameDocument(vm, tb.Text ?? string.Empty);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.IsEditing = false; // 不改名，直接退出
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// 树选中变化：同步到 <see cref="MainViewModel.SelectedDocument"/>；若为右键导致的选中，按标记抑制预览联动。
    /// </summary>
    private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressPreviewByRightClick)
        {
            _suppressPreviewByRightClick = false; // 重置标记，避免影响下一次
            return; // 右键导致的选中变化不触发 SelectedDocument/预览
        }

        if (sender is TreeView treeView && treeView.SelectedItem is DocumentViewModel documentViewModel)
        {
            _viewModel!.SelectedDocument = documentViewModel;
            Console.WriteLine($"[DocumentTreeView] TreeView选中: {documentViewModel.Name}, Path={documentViewModel.Path}");
            // 如需“选中即预览”，可在此调用 _viewModel.PreviewDocument1(documentViewModel)（当前保持注释）
        }
    }

    /// <summary>
    /// 执行重命名（文件或文件夹）：基本校验 → 文件系统移动 → VM 同步路径与名称 → 视图中选中并聚焦该项。
    /// </summary>
    /// <param name="vm">目标视图模型。</param>
    /// <param name="newName">新名称。</param>
    private void RenameDocument(DocumentViewModel vm, string newName)
    {
        // 1) 基本校验
        if (string.IsNullOrWhiteSpace(newName) || newName == vm.Name)
        {
            Console.WriteLine("[重命名] 新名称为空或与当前名称相同，跳过重命名");
            return;
        }

        var parentDir = System.IO.Path.GetDirectoryName(vm.Path);
        if (parentDir == null)
        {
            Console.WriteLine("[重命名] 获取父目录失败，无法重命名");
            return;
        }

        var newPath = System.IO.Path.Combine(parentDir, newName);

        try
        {
            // 2) 文件系统移动（区分文件/文件夹）
            if (vm.IsFolder)
            {
                if (System.IO.Directory.Exists(vm.Path))
                {
                    System.IO.Directory.Move(vm.Path, newPath);
                }
                else
                {
                    Console.WriteLine("[重命名] 当前路径不是有效文件夹");
                    return;
                }
            }
            else
            {
                if (System.IO.File.Exists(vm.Path))
                {
                    System.IO.File.Move(vm.Path, newPath);
                }
                else
                {
                    Console.WriteLine("[重命名] 当前路径不是有效文件");
                    return;
                }
            }

            // 3) 同步 VM 的路径与名称
            vm.ChangePath(newPath);
            vm.Rename(newName);
            Console.WriteLine($"[重命名] 成功: {vm.Name} -> {newName}");

            // 4) 刷新并选中该项，使之获得焦点
            var treeView = this.Find<TreeView>("Tree");
            if (treeView != null)
            {
                treeView.SelectedItem = vm;

                var treeViewItem = treeView.GetVisualDescendants()
                    .OfType<TreeViewItem>()
                    .FirstOrDefault(item => item.DataContext == vm);
                treeViewItem?.Focus();
            }

            if (_viewModel != null)
            {
                _viewModel.SelectedDocument = vm;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[重命名] 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 快捷键输入框 KeyDown：将当前组合键转为字符串；若已有内容则以“, ”追加，形成“Ctrl+K, Ctrl+C”形式。
    /// </summary>
    /// <remarks>
    /// 规则：
    /// - 忽略纯修饰键（Left/Right Ctrl/Shift/Alt）；  
    /// - 以 <c>Ctrl+</c>、<c>Shift+</c>、<c>Alt+</c> 前缀拼接，再追加主键名；  
    /// - 支持连续录入，用逗号+空格分隔多个步骤。  
    /// </remarks>
    private void ShortcutInputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        // 忽略纯修饰键
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
            e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            e.Handled = true;
            return;
        }

        // 组装当前组合键字符串
        var sb = new StringBuilder();
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            sb.Append("Ctrl+");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            sb.Append("Shift+");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            sb.Append("Alt+");

        sb.Append(e.Key.ToString());

        // 赋值到文本框；已有内容则追加“, ”
        if (sender is TextBox tb)
        {
            tb.Text = string.IsNullOrWhiteSpace(tb.Text) ? sb.ToString() : (tb.Text + ", " + sb.ToString());
            // 如需同步到 VM，可在外部绑定命令或属性
        }

        e.Handled = true;
    }
}
