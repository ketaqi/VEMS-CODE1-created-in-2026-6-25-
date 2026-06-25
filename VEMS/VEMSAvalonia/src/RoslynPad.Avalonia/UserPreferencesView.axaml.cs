using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RoslynPad.UI;
using Avalonia.VisualTree;
using System.Text;

namespace RoslynPad;

/// <summary>
/// 文档树视图（第 1 区域）：
/// - 支持双击/Enter 打开文档；
/// - 支持右键菜单“打开/展开/折叠”与“内联重命名”；
/// - 提供“快捷键输入框”与“路径输入框”的键盘录入行为。
/// </summary>
/// <remarks>
/// 事件源可能为任意子元素（图标/文本/面板等），通过从 <see cref="RoutedEventArgs.Source"/> 沿视觉树向上查找最近的
/// <see cref="TreeViewItem"/> 来获取其 <c>DataContext</c>（<see cref="DocumentViewModel"/>）。  
/// 线程模型：仅进行 UI 线程事件处理；文件系统重命名使用简单同步调用。  
/// 安全性：重命名前做空字符串与同名短路；失败捕获异常并写入控制台日志。  
/// </remarks>
public partial class UserPreferencesView : UserControl
{
    /// <summary>当前页面绑定的主视图模型。</summary>
    private MainViewModel? _viewModel;

    /// <summary>
    /// 构造：初始化组件，查找模板元素，并挂接交互事件（树视图、快捷键输入框、路径输入框）。
    /// </summary>
    public UserPreferencesView()
    {
        InitializeComponent();

        // 1) 树：双击/Enter 打开文档
        var treeView = this.Find<TreeView>("Tree");
        if (treeView != null)
        {
            treeView.DoubleTapped += OnDocumentClick;
            treeView.KeyDown += OnDocumentKeyDown;
        }

        // 2) 快捷键输入框：按键录入 + 文本输入（用于英文逗号等）
        var shortcutInputBox = this.Find<TextBox>("ShortcutInputBox");
        if (shortcutInputBox != null)
        {
            shortcutInputBox.KeyDown += ShortcutInputBox_KeyDown;
            shortcutInputBox.TextInput += ShortcutInputBox_TextInput;
        }

        // 3) 路径输入框：Enter 切换目录、Esc 清空
        var pathInputBox = this.Find<TextBox>("PathInputBox");
        if (pathInputBox != null)
        {
            pathInputBox.KeyDown += PathInputBox_KeyDown;
        }
    }

    /// <summary>
    /// 文本输入事件：允许在快捷键输入框录入英文逗号，用作“多步快捷键”的分隔符提示。
    /// </summary>
    private void ShortcutInputBox_TextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (e.Text == ",")
            {
                // 允许逗号输入，不做拦截（保留日志提示即可）
                Console.WriteLine("已输入英文逗号分隔，如需多步快捷键请继续输入下一步。");
                // 如需自动补空格等格式化，可在此扩展
            }
        }
    }

    /// <summary>
    /// DataContext 变化时缓存 <see cref="MainViewModel"/> 实例，供后续调用。
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext != null)
            _viewModel = DataContext as MainViewModel;
    }

    /// <summary>
    /// 树节点双击：尝试打开对应文档。
    /// </summary>
    private void OnDocumentClick(object? sender, RoutedEventArgs e)
    {
        OpenDocument(e.Source);
    }

    /// <summary>
    /// 树节点按键：Enter 打开对应文档。
    /// </summary>
    private void OnDocumentKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OpenDocument(e.Source);
        }
    }

    /// <summary>
    /// 从事件源沿视觉树向上定位最近的 <see cref="TreeViewItem"/>，若 DataContext 为
    /// <see cref="DocumentViewModel"/>，则调用主 VM 执行打开或目录展开。
    /// </summary>
    private void OpenDocument(object? source)
    {
        // 事件源可能不是 TreeViewItem：沿视觉树查找最近的 TreeViewItem
        var item = (source as Visual)?.GetSelfAndVisualAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();

        if (item?.DataContext is DocumentViewModel documentViewModel)
        {
            _viewModel?.OpenDocument(documentViewModel);
        }
    }

    /// <summary>
    /// 右键菜单“打开”项：若目标为文件夹则切换展开/折叠；若为文件则打开。
    /// </summary>
    /// <remarks>
    /// 尝试优先通过 <see cref="ContextMenu.PlacementTarget"/> 获取 <see cref="DockPanel"/>，
    /// 若失败再从视觉树回溯查找；随后从 <c>DockPanel.DataContext</c> 读取 VM。
    /// </remarks>
    private void MenuItem_Open_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            // 优先：由 ContextMenu 定位目标 DockPanel
            var contextMenu = menuItem.Parent as ContextMenu;
            var dockPanel = contextMenu?.PlacementTarget as DockPanel;

            // 回退：视觉树查找
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

            // 文件夹：切换展开；文件：打开
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
    /// 右键菜单“重命名”项：进入内联编辑模式（切换到文本框）。
    /// </summary>
    private void OnInlineRenameMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is DocumentViewModel vm)
        {
            vm.IsEditing = true;
        }
    }

    ///// <summary>
    ///// 可选：双击标签文本进入重命名（当前保留注释，避免与“双击打开”冲突）。
    ///// </summary>
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
    /// 重命名输入框按键：Enter 提交重命名。
    /// </summary>
    private void OnInlineRenameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox tb && tb.DataContext is DocumentViewModel vm)
        {
            vm.IsEditing = false;
            RenameDocument(vm, tb.Text ?? string.Empty); // 保证 newName 非 null
        }
    }

    /// <summary>
    /// 树选择变化：同步 <see cref="MainViewModel.SelectedDocument"/> 并打印日志。
    /// </summary>
    private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is TreeView treeView && treeView.SelectedItem is DocumentViewModel documentViewModel)
        {
            _viewModel!.SelectedDocument = documentViewModel;
            Console.WriteLine($"[DocumentTreeView] TreeView选中: {documentViewModel.Name}, Path={documentViewModel.Path}");
        }
    }

    /// <summary>
    /// 执行重命名（文件或文件夹）：进行基本校验并调用文件系统移动，然后通过 VM 同步路径/名称并选中该项。
    /// </summary>
    /// <param name="vm">目标文档/文件夹的视图模型。</param>
    /// <param name="newName">新的文件/文件夹名。</param>
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
            // 2) 文件系统移动
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

            // 3) 用 VM 对外方法同步最新路径与名称
            vm.ChangePath(newPath);
            vm.Rename(newName);
            Console.WriteLine($"[重命名] 成功: {vm.Name} -> {newName}");

            // 4) 刷新并选中该项
            var treeView = this.Find<TreeView>("Tree");
            if (treeView != null)
            {
                treeView.SelectedItem = vm;

                // 通过视觉树找到对应的 TreeViewItem 并设置焦点
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

    // ———————————————————— 快捷键录入 ————————————————————

    /// <summary>
    /// 快捷键输入框 KeyDown：将当前组合键转为“Ctrl+Shift+K”形式写回文本框；Enter 校验格式；Esc 清空。
    /// </summary>
    /// <remarks>
    /// 规则：
    /// - 忽略纯修饰键（仅按下 Ctrl/Shift/Alt 时不记录）；  
    /// - Enter：若非空则校验 <see cref="MainViewModel.IsValidShortcut(string)"/>，通过则确认，否则清空并提示；  
    /// - 其余按键：根据修饰键组装后覆盖文本框内容。  
    /// </remarks>
    private void ShortcutInputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        // Esc：取消录入
        if (e.Key == Key.Escape)
        {
            tb.Text = string.Empty;
            Console.WriteLine("已取消快捷键录入。");
            e.Handled = true;
            return;
        }

        // 忽略仅修饰键（避免出现“Ctrl+”无主键）
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
            e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            Console.WriteLine("请按下功能键或字母键作为快捷键（如 Ctrl+K）");
            e.Handled = true;
            return;
        }

        // Enter：校验并确认
        if (e.Key == Key.Enter)
        {
            var shortcutStr = (tb.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(shortcutStr))
            {
                if (MainViewModel.IsValidShortcut(shortcutStr))
                {
                    Console.WriteLine($"快捷键录入完成：{shortcutStr}，已确认。");
                }
                else
                {
                    Console.WriteLine($"[快捷键错误] 格式无效: {shortcutStr}，请重新输入！格式示例：Ctrl+K");
                    tb.Text = string.Empty;
                }
            }
            else
            {
                Console.WriteLine("请先输入快捷键。格式示例：Ctrl+K");
            }
            e.Handled = true;
            return;
        }

        // 组装当前组合键字符串（Ctrl/Shift/Alt + Key）
        var sb = new StringBuilder();
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            sb.Append("Ctrl+");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            sb.Append("Shift+");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            sb.Append("Alt+");

        sb.Append(e.Key.ToString());

        // 单步覆盖：每次按键都直接替换文本框内容
        tb.Text = sb.ToString();
        Console.WriteLine($"已输入：{sb}，输入完成后请按回车键确认。格式示例：Ctrl+K");

        e.Handled = true;
    }

    // ———————————————————— 路径录入 ————————————————————

    /// <summary>
    /// 路径输入框 KeyDown：Enter 尝试切换到该目录；Esc 清空输入。
    /// </summary>
    private void PathInputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        // Enter：校验本地路径并调用主 VM 切换
        if (e.Key == Key.Enter)
        {
            var inputPath = tb.Text?.Trim();
            if (!string.IsNullOrEmpty(inputPath))
            {
                if (System.IO.Directory.Exists(inputPath))
                {
                    _viewModel?.OpenFolderByPath1(inputPath);
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
            e.Handled = true;
            return;
        }

        // Esc：清空输入
        if (e.Key == Key.Escape)
        {
            tb.Text = string.Empty;
            Console.WriteLine("[路径输入] 已清空输入框");
            e.Handled = true;
            return;
        }
        TextBox _tb = new TextBox();
        HandleShortcutKey(_tb, Key.K, KeyModifiers.Control);
        string shortcut = GetShortcutString(Key.K, KeyModifiers.Control);
        
    }


    public string HandleShortcutKey(TextBox tb, Key key, KeyModifiers modifiers)
    {
        string shortcut = GetShortcutString(key, modifiers);
        tb.Text = shortcut;
        return shortcut;
    }
    public string GetShortcutString(Key key, KeyModifiers modifiers)
    {
        // 1. 忽略仅修饰键
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LeftAlt || key == Key.RightAlt)
        {
            return ""; // 仅修饰键时返回空字符串
        }

        // 2. 组装组合键字符串
        var sb = new StringBuilder();
        if (modifiers.HasFlag(KeyModifiers.Control))
            sb.Append("Ctrl+");
        if (modifiers.HasFlag(KeyModifiers.Shift))
            sb.Append("Shift+");
        if (modifiers.HasFlag(KeyModifiers.Alt))
            sb.Append("Alt+");

        sb.Append(key.ToString());

        return sb.ToString();
    }
}
