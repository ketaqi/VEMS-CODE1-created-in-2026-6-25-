using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Avalonia.Controls.ApplicationLifetimes;
using System.Windows.Input;
using RoslynPad.Build;
using Avalonia;
using RoslynPad.UI.Dialogs;
using RoslynPad.Roslyn;
using RoslynPad.ViewModels;
using RoslynPad.Utilities;
using NotificationObject = RoslynPad.ViewModels.NotificationObject;

namespace RoslynPad.UI;

/// <summary>
/// 文档/目录树节点的 ViewModel。
/// <para>既可表示文件，也可表示文件夹。</para>
/// <para>负责：子节点读取、排序、搜索可见性、创建/删除、剪贴板操作等。</para>
/// </summary>
[DebuggerDisplay("{Name}:{IsFolder}")]
public partial class DocumentViewModel : NotificationObject
{
    /// <summary>
    /// 自动保存文件名后缀（用于内部 autosave 文件）。
    /// </summary>
    internal const string AutoSaveSuffix = ".autosave";

    /// <summary>
    /// “资源管理器/目录树”中认为是相关文件的扩展名列表。
    /// <para>用于读取文件夹下的可展示文件。</para>
    /// </summary>
    public static ImmutableArray<string> RelevantFileExtensions { get; } = [
        ".cs", ".csx", ".xml", ".json", ".axaml",
        ".md", ".dockerfile", ".bat", ".ps1", ".sql",
        ".gitignore", ".gitconfig", ".gitattributes",
        ".yml", ".yaml", ".ini", ".toml", ".sh", ".py", ".rb", ".php", ".pl",
        ".js", ".html", ".css", ".java", ".c", ".cpp"
    ];

    private bool _isExpanded;
    private bool? _isAutoSaveOnly;
    private bool _isSearchMatch;
    private string? _path;
    private string? _name;
    private string? _orderByName;

    /// <summary>
    /// 父节点（用于向上刷新可见性等）。
    /// </summary>
    public DocumentViewModel? Parent { get; private set; }

    /// <summary>
    /// 构造函数：根据路径与类型创建节点。
    /// <para>注意：这是私有构造，外部通过 CreateRoot / FromPath / CreateNew 创建。</para>
    /// </summary>
    private DocumentViewModel(string rootPath, bool isFolder)
    {
        Path = rootPath;
        IsFolder = isFolder;

        // autosave 文件名规则处理：若文件名（不含扩展名）以 .autosave 结尾，则标记为 autosave，
        // 并将显示名 Name 恢复为原始文件名（去掉 autosave 后缀）。
        var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(Name);
        IsAutoSave = nameWithoutExtension.EndsWith(AutoSaveSuffix, StringComparison.OrdinalIgnoreCase);
        if (IsAutoSave)
        {
            Name = string.Concat(
                nameWithoutExtension.AsSpan(0, nameWithoutExtension.Length - AutoSaveSuffix.Length),
                System.IO.Path.GetExtension(Name));
        }

        // 默认认为搜索匹配（即默认可见）
        IsSearchMatch = true;
    }

    /// <summary>
    /// 修改当前节点路径，并清空排序缓存。
    /// <para>用于重命名/移动等场景后刷新。</para>
    /// </summary>
    public void ChangePath(string newPath)
    {
        Path = newPath;
        _orderByName = null;
    }

    /// <summary>
    /// 当前节点的全路径。
    /// <para>设置后会同步更新 Name；若节点为文件夹且已初始化子节点，会递归更新子节点路径。</para>
    /// </summary>
    public string Path
    {
        get => _path.NotNull();
        [MemberNotNull(nameof(_path))]
        private set
        {
            var oldPath = _path;
            if (SetProperty(ref _path, value))
            {
                Name = System.IO.Path.GetFileName(value);
                if (oldPath is not null)
                {
                    UpdateChildPaths(oldPath, value);
                }
            }
        }
    }

    /// <summary>
    /// 当文件夹路径变化时，递归替换子节点的路径前缀。
    /// <para>仅在文件夹且子节点已初始化时执行。</para>
    /// </summary>
    private void UpdateChildPaths(string oldPath, string newPath)
    {
        if (!IsFolder || !IsChildrenInitialized)
        {
            return;
        }

        foreach (var child in InternalChildren!)
        {
            child.Path = child.Path.Replace(oldPath, newPath);
        }
    }

    /// <summary>
    /// 是否为文件夹节点；否则为文件节点。
    /// </summary>
    public bool IsFolder { get; }

    /// <summary>
    /// 获取用于“保存”的实际文件路径：
    /// <para>若当前节点是 autosave 文件，则返回对应原始文件路径；否则返回自身 Path。</para>
    /// </summary>
    public string GetSavePath()
    {
        return IsAutoSave
            ? System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, Name)
            : Path;
    }

    /// <summary>
    /// 获取 autosave 文件路径：
    /// <para>若当前节点已经是 autosave 文件，则直接返回自身 Path；否则返回同目录下 autosave 名称对应路径。</para>
    /// </summary>
    public string GetAutoSavePath()
    {
        return IsAutoSave
            ? Path
            : System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, GetAutoSaveName(Name));
    }

    /// <summary>
    /// 由原文件名生成 autosave 文件名（保持原扩展名）。
    /// </summary>
    public static string GetAutoSaveName(string name)
    {
        return System.IO.Path.ChangeExtension(name, AutoSaveSuffix + System.IO.Path.GetExtension(name));
    }

    /// <summary>
    /// 创建根目录节点，并确保根目录存在。
    /// </summary>
    public static DocumentViewModel CreateRoot(string rootPath)
    {
        IOUtilities.PerformIO(() => Directory.CreateDirectory(rootPath));
        return new DocumentViewModel(rootPath, true);
    }

    /// <summary>
    /// 由路径创建节点；若路径存在且为目录则为文件夹节点，否则为文件节点。
    /// </summary>
    public static DocumentViewModel FromPath(string path)
    {
        return new DocumentViewModel(path, isFolder: Directory.Exists(path));
    }

    /// <summary>
    /// 在当前文件夹节点下创建新的“文件节点”（仅创建 VM，不在磁盘写入）。
    /// </summary>
    /// <param name="documentName">文件名（相对当前文件夹）。</param>
    public DocumentViewModel CreateNew(string documentName)
    {
        if (!IsFolder) throw new InvalidOperationException("Parent must be a folder");

        var document = new DocumentViewModel(GetDocumentPathFromName(Path, documentName), false)
        {
            // 维护父子关系
            Parent = this
        };
        AddChild(document);
        return document;
    }

    /// <summary>
    /// 将“父目录路径 + 名称”拼接为完整路径。
    /// </summary>
    public static string GetDocumentPathFromName(string path, string name) =>
        System.IO.Path.Combine(path, name);

    /// <summary>
    /// 删除 autosave 文件：
    /// <para>如果当前节点本身是 autosave，则删除自身 Path；否则删除对应 autosave 路径（若存在）。</para>
    /// </summary>
    public void DeleteAutoSave()
    {
        if (IsAutoSave)
        {
            IOUtilities.PerformIO(() => File.Delete(Path));
        }
        else
        {
            var autoSavePath = GetAutoSavePath();
            if (File.Exists(autoSavePath))
            {
                IOUtilities.PerformIO(() => File.Delete(autoSavePath));
            }
        }
    }

    /// <summary>
    /// TreeView 是否展开（UI 状态）。
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    /// <summary>
    /// 节点名称（文件名或目录名，用于显示）。
    /// </summary>
    public string Name
    {
        get => _name.NotNull();
        private set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// 当前节点是否是 autosave 文件节点。
    /// </summary>
    public bool IsAutoSave { get; }

    /// <summary>
    /// 是否“仅存在 autosave 文件而不存在原文件”的节点。
    /// <para>用于决定 UI 或过滤逻辑。</para>
    /// </summary>
    public bool IsAutoSaveOnly =>
        _isAutoSaveOnly ??= IsAutoSave &&
            !File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, Name));

    /// <summary>
    /// 子节点集合是否已初始化（即已读取过目录内容）。
    /// </summary>
    [MemberNotNullWhen(true, nameof(InternalChildren))]
    public bool IsChildrenInitialized => InternalChildren != null;

    /// <summary>
    /// 内部子节点集合（自定义集合类型）。
    /// </summary>
    internal DocumentCollection? InternalChildren { get; private set; }

    /// <summary>
    /// 子节点集合（用于绑定）。
    /// <para>首次访问时会读取目录并创建子节点。</para>
    /// </summary>
    public ObservableCollection<DocumentViewModel>? Children
    {
        get
        {
            if (IsFolder && InternalChildren == null)
            {
                InternalChildren = ReadChildren();

                // 首次读取时维护父子关系
                foreach (var c in InternalChildren)
                    c.Parent = this;
            }

            return InternalChildren;
        }
    }

    /// <summary>
    /// 搜索匹配状态：用于控制节点是否被搜索过滤命中。
    /// <para>当值变化时，会刷新自身及祖先节点的可见性，并向下刷新子树。</para>
    /// </summary>
    public bool IsSearchMatch
    {
        get => _isSearchMatch;
        internal set
        {
            if (SetProperty(ref _isSearchMatch, value))
            {
                // 匹配状态变化 => 刷新自身/父级可见性及可见子节点集合
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(VisibleChildren));
                Parent?.RefreshSearchVisibilityUpward();

                // 若为文件夹且已读取子节点，则向下刷新
                if (IsFolder && IsChildrenInitialized)
                {
                    foreach (var c in InternalChildren!)
                        c.RefreshSearchVisibility();
                }
            }
        }
    }

    /// <summary>
    /// 节点是否可见
    /// <para>自身匹配 或（为文件夹且存在任一可见子节点）。</para>
    /// </summary>
    public bool IsVisible =>
        IsSearchMatch || (IsFolder && (Children?.Any(c => c.IsVisible) ?? false));

    /// <summary>
    /// 仅返回“可见”的子节点集合（用于 TreeView 过滤显示）。
    /// </summary>
    public IEnumerable<DocumentViewModel> VisibleChildren =>
        (Children ?? Enumerable.Empty<DocumentViewModel>()).Where(c => c.IsVisible);

    /// <summary>
    /// 读取当前目录下子文件夹与文件，生成子节点集合并排序。
    /// <para>文件会按 RelevantFileExtensions 过滤；autosave 文件会被排除。</para>
    /// </summary>
    private DocumentCollection ReadChildren()
    {
        var directories =
            IOUtilities.EnumerateDirectories(Path)
            .Select(directory => new DocumentViewModel(directory, isFolder: true))
            .OrderBy(directory => directory.OrderByName);

        var files = Enumerable.Empty<DocumentViewModel>();

        foreach (var extension in RelevantFileExtensions)
        {
            files = files.Concat(IOUtilities.EnumerateFiles(Path, "*" + extension)
                .Select(file => new DocumentViewModel(file, isFolder: false))
                .Where(file => !file.IsAutoSave));
        }

        // 合并并排序：文件夹在前（上面已排序），文件按名称排序
        var collection = new DocumentCollection(directories.Concat(files.OrderBy(file => file.OrderByName)));

        // 维护父子关系
        foreach (var c in collection)
            c.Parent = this;

        return collection;
    }

    /// <summary>
    /// 用于排序的名称：
    /// <para>将名称中数字部分填充为定长（PadLeft），使 “file2” 在 “file10” 前。</para>
    /// </summary>
    private string OrderByName =>
        _orderByName ??= NumberRegex().Replace(Name, m => m.Value.PadLeft(100, '0'));

    /// <summary>
    /// 将子节点插入到当前节点的子集合中，并保持排序。
    /// <para>若已存在同名节点，则只更新其 MainViewModel、父关系并刷新可见集合。</para>
    /// </summary>
    internal void AddChild(DocumentViewModel documentViewModel)
    {
        ArgumentNullException.ThrowIfNull(documentViewModel);

        // 确保子集合已初始化
        if (InternalChildren is null)
        {
            _ = Children; // 触发读取（如为文件夹）
            InternalChildren ??= new DocumentCollection(Enumerable.Empty<DocumentViewModel>());
        }

        var children = InternalChildren;

        // 若存在同名节点，避免重复插入，补全关系/上下文并刷新
        var existing = children[documentViewModel.Name];
        if (existing is not null)
        {
            existing.Parent = this;
            existing.MainViewModel ??= this.MainViewModel ?? this.Parent?.MainViewModel;
            IsExpanded = true;
            OnPropertyChanged(nameof(VisibleChildren));
            return;
        }

        // 计算插入位置：同类型（文件夹/文件）按 OrderByName 排序
        int insertIndex = -1;
        for (int i = 0; i < children.Count; i++)
        {
            var d = children[i];
            if (d.IsFolder == documentViewModel.IsFolder &&
                string.Compare(documentViewModel.OrderByName, d.OrderByName, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                insertIndex = i;
                break;
            }
        }

        // 若未找到同类型插入点：
        // - 文件夹：插到第一个文件前（保持文件夹在前）
        // - 文件：追加到末尾
        if (insertIndex < 0)
        {
            if (documentViewModel.IsFolder)
            {
                int firstFile = -1;
                for (int i = 0; i < children.Count; i++)
                    if (!children[i].IsFolder) { firstFile = i; break; }

                insertIndex = firstFile >= 0 ? firstFile : children.Count;
            }
            else
            {
                insertIndex = children.Count;
            }
        }

        // 维护关系/上下文并插入
        documentViewModel.Parent = this;
        documentViewModel.MainViewModel ??= this.MainViewModel ?? this.Parent?.MainViewModel;
        children.Insert(insertIndex, documentViewModel);

        // 刷新可见子集合并自动展开
        OnPropertyChanged(nameof(VisibleChildren));
        IsExpanded = true;

        // 原代码中存在重复的 ThrowIfNull 与 MainViewModel 赋值；
        // 这里保留不改动，以确保行为完全一致。
        ArgumentNullException.ThrowIfNull(documentViewModel);
        documentViewModel.MainViewModel ??= this.MainViewModel ?? this.Parent?.MainViewModel;
    }

    /// <summary>
    /// 刷新当前节点及其子树的搜索可见性相关属性通知。
    /// <para>用于某个节点匹配状态变化后向下刷新。</para>
    /// </summary>
    public void RefreshSearchVisibility()
    {
        OnPropertyChanged(nameof(IsVisible));
        OnPropertyChanged(nameof(VisibleChildren));
        if (IsFolder && IsChildrenInitialized)
        {
            foreach (var c in InternalChildren!)
                c.RefreshSearchVisibility();
        }
    }

    /// <summary>
    /// 向上递归刷新祖先节点的搜索可见性相关属性通知。
    /// <para>用于子节点匹配状态变化后向上刷新。</para>
    /// </summary>
    public void RefreshSearchVisibilityUpward()
    {
        OnPropertyChanged(nameof(IsVisible));
        OnPropertyChanged(nameof(VisibleChildren));
        Parent?.RefreshSearchVisibilityUpward();
    }

    /// <summary>
    /// 数字匹配正则：用于排序时对数字部分进行对齐。
    /// </summary>
    [GeneratedRegex("[0-9]+")]
    private static partial Regex NumberRegex();

    // ===================== 右键菜单/辅助命令相关 =====================

    private ICommand? _copyFilePathCommand;

    /// <summary>
    /// 复制当前节点（文件/文件夹）的 Path 到剪贴板。
    /// </summary>
    public ICommand CopyFilePathCommand => _copyFilePathCommand ??= new RelayCommand(_ => CopyFilePath());

    /// <summary>
    /// 执行复制文件路径到剪贴板（异步）。
    /// <para>优先使用当前活动窗口的 Clipboard。</para>
    /// </summary>
    private async void CopyFilePath()
    {
        if (!string.IsNullOrEmpty(Path))
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.Windows.FirstOrDefault(w => w.IsActive)
                : null;

            if (window != null)
            {
                var clipboard = window.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(Path).ConfigureAwait(true);
                    Console.WriteLine($"已复制文件路径: {Path}");
                }
                else
                {
                    Console.WriteLine("未能获取剪贴板（Clipboard 为 null）。");
                }
            }
            else
            {
                Console.WriteLine("未找到活动窗口，无法初始化剪贴板操作。");
            }
        }
    }

    /// <summary>
    /// 在系统文件管理器中打开当前节点所在文件夹。
    /// <para>若节点本身为文件夹，则打开该文件夹；否则打开文件所在目录。</para>
    /// </summary>
    private void OpenContainingFolder()
    {
        try
        {
            // 获取文件夹路径
            var folderPath = IsFolder ? Path : System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                Console.WriteLine("文件夹路径无效或不存在: " + folderPath);
                return;
            }

            // 按平台打开文件夹
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folderPath}\"") { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", folderPath);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", folderPath);
            }
            Console.WriteLine($"已打开文件夹: {folderPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("打开文件夹失败: " + ex.Message);
        }
    }

    private ICommand? _openContainingFolderCommand;

    /// <summary>
    /// 打开所在文件夹命令。
    /// </summary>
    public ICommand OpenContainingFolderCommand => _openContainingFolderCommand ??= new RelayCommand(_ => OpenContainingFolder());

    /// <summary>
    /// 将指定文本复制到剪贴板（异步）。
    /// </summary>
    public async void CopyTextToClipboard(string text)
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.Windows.FirstOrDefault(w => w.IsActive)
            : null;

        if (window != null)
        {
            var clipboard = window.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text).ConfigureAwait(true);
                Console.WriteLine("[CopyTextToClipboard] 已复制到剪贴板: " + text);

                var clipboardText = await clipboard.GetTextAsync().ConfigureAwait(true);
                Console.WriteLine("[CopyTextToClipboard] 剪贴板当前内容: " + clipboardText);
            }
            else
            {
                Console.WriteLine("[CopyTextToClipboard] 未能获取剪贴板（Clipboard 为 null）。");
            }
        }
        else
        {
            Console.WriteLine("[CopyTextToClipboard] 未找到活动窗口，无法初始化剪贴板操作。");
        }
    }

    /// <summary>
    /// 复制当前节点所在文件夹路径到剪贴板（异步）。
    /// <para>文件节点复制其目录；文件夹节点复制自身路径。</para>
    /// </summary>
    public async void CopyFolderPathToClipboard()
    {
        var folderPath = IsFolder ? Path : System.IO.Path.GetDirectoryName(Path);
        if (string.IsNullOrEmpty(folderPath))
        {
            Console.WriteLine("[CopyFolderPathToClipboard] 文件夹路径无效");
            return;
        }

        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.Windows.FirstOrDefault(w => w.IsActive)
            : null;

        if (window != null)
        {
            var clipboard = window.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(folderPath).ConfigureAwait(true);
                Console.WriteLine("[CopyFolderPathToClipboard] 已复制文件夹路径: " + folderPath);

                var clipboardText = await clipboard.GetTextAsync().ConfigureAwait(true);
                Console.WriteLine("[CopyFolderPathToClipboard] 剪贴板当前内容: " + clipboardText);
            }
            else
            {
                Console.WriteLine("[CopyFolderPathToClipboard] 未能获取剪贴板（Clipboard 为 null）。");
            }
        }
        else
        {
            Console.WriteLine("[CopyFolderPathToClipboard] 未找到活动窗口，无法初始化剪贴板操作。");
        }
    }

    private ICommand? _copyFolderPathCommand;

    /// <summary>
    /// 复制文件夹路径命令。
    /// </summary>
    public ICommand CopyFolderPathCommand => _copyFolderPathCommand ??= new RelayCommand(_ => CopyFolderPathToClipboard());

    private ICommand? _createFileCommand;

    /// <summary>
    /// 在当前文件夹下创建新文件的命令（会写入磁盘）。
    /// <para>通过输入对话框获取文件名。</para>
    /// </summary>
    public ICommand CreateFileCommand => _createFileCommand ??= new RelayCommand(async _ =>
    {
        if (!IsFolder)
        {
            MainViewModel?.OutputResult("[提示]", "只能在文件夹下新建文件", "提示", null);
            return;
        }

        // 获取当前活动窗口
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.Windows.FirstOrDefault(w => w.IsActive)
            : null;

        string? fileName = null;

        // 使用输入对话框获取文件名
        if (window != null)
        {
            var inputDialog = new InputDialog("新建文件", "请输入文件名", "新建文件.cs");
            fileName = await inputDialog.ShowDialogAsync(window).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // 用户取消或未输入
                return;
            }
        }
        else
        {
            fileName = "新建文件.cs";
        }

        // 校验文件名
        if (string.IsNullOrWhiteSpace(fileName))
        {
            MainViewModel?.OutputResult("[提示]", "文件名不能为空", "提示", null);
            return;
        }

        // 判断是否已存在
        var filePath = GetDocumentPathFromName(Path, fileName);
        if (File.Exists(filePath))
        {
            MainViewModel?.OutputResult("[提示]", $"文件已存在: {fileName}", "提示", null);
            return;
        }

        // 创建文件并添加到树
        File.WriteAllText(filePath, string.Empty);
        var newFile = new DocumentViewModel(filePath, false) { Parent = this };
        AddChild(newFile);
        Console.WriteLine($"[DocumentViewModel] 新建文件: {filePath}");
    });

    /// <summary>
    /// 主视图模型引用（由外部注入），用于输出提示、访问打开文档列表等。
    /// </summary>
    public MainViewModel? MainViewModel { get; set; }

    /// <summary>
    /// 生成不与现有子节点/磁盘冲突的唯一名称。
    /// <para>若冲突则自动追加 “ 1 / 2 / 3 ...”。</para>
    /// </summary>
    private string GetUniqueChildName(string baseName, bool isFolder)
    {
        // 使用 OrdinalIgnoreCase 进行不区分大小写比较
        var existing = new HashSet<string>(
            (Children ?? Enumerable.Empty<DocumentViewModel>()).Select(c => c.Name),
            StringComparer.OrdinalIgnoreCase);

        string candidate = baseName;
        int i = 0;
        while (true)
        {
            var path = System.IO.Path.Combine(Path, candidate);
            bool existsOnDisk = isFolder ? Directory.Exists(path) : File.Exists(path);

            if (!existing.Contains(candidate) && !existsOnDisk)
                return candidate;

            i++;
            candidate = $"{baseName} {i}";
        }
    }

    /// <summary>
    /// 在当前文件夹下创建新文件夹（会写入磁盘并插入树）。
    /// </summary>
    public void CreateFolder(string folderName)
    {
        if (!IsFolder)
        {
            MainViewModel?.OutputResult("[提示]", "只能在文件夹下新建文件夹", "提示", null);
            return;
        }

        // 生成不冲突的文件夹名
        var uniqueName = GetUniqueChildName(folderName, isFolder: true);
        var folderPath = GetDocumentPathFromName(Path, uniqueName);

        Directory.CreateDirectory(folderPath);

        var newFolder = new DocumentViewModel(folderPath, true)
        {
            Parent = this,
            // 传递 MainViewModel 上下文
            MainViewModel = this.MainViewModel ?? this.Parent?.MainViewModel
        };

        AddChild(newFolder);

        // 进入编辑状态（用于 UI 重命名输入）
        newFolder.IsEditing = true;

        // 将新建节点设为选中
        var owner = this.MainViewModel ?? this.Parent?.MainViewModel;
        if (owner != null)
            owner.SelectedDocument = newFolder;

        Console.WriteLine($"[DocumentViewModel] 新建文件夹: {folderPath}");
    }

    /// <summary>
    /// 在当前文件夹下创建新文件（会写入磁盘并插入树）。
    /// <para>若名称冲突会自动生成唯一名称。</para>
    /// </summary>
    public void CreateFile(string fileName)
    {
        if (!IsFolder)
        {
            MainViewModel?.OutputResult("[提示]", "只能在文件夹下新建文件", "提示", null);
            return;
        }

        // 生成不冲突的文件名
        var uniqueName = GetUniqueChildName(fileName, isFolder: false);
        var filePath = GetDocumentPathFromName(Path, uniqueName);

        File.WriteAllText(filePath, string.Empty);

        var newFile = new DocumentViewModel(filePath, false) { Parent = this };
        AddChild(newFile);

        // 进入编辑状态（用于 UI 重命名输入）
        newFile.IsEditing = true;

        Console.WriteLine($"[DocumentViewModel] 新建文件: {filePath}");
    }

    private ICommand? _createFolderCommand;

    /// <summary>
    /// 新建文件夹命令（默认名称）。
    /// </summary>
    public ICommand CreateFolderCommand => _createFolderCommand ??= new RelayCommand(_ =>
    {
        CreateFolder("新建文件夹");
    });

    private bool _isEditing;

    /// <summary>
    /// 是否处于“重命名/编辑名称”状态（用于 UI）。
    /// </summary>
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    /// <summary>
    /// 删除当前节点：
    /// <list type="number">
    /// <item>若删除文件夹：先关闭其子树下所有已打开文档。</item>
    /// <item>删除磁盘上的文件/文件夹。</item>
    /// <item>从父节点 Children 中移除，并刷新父节点可见性。</item>
    /// </list>
    /// </summary>
    public void Delete()
    {
        try
        {
            // 1. 关闭相关已打开文档
            var mainVM = this.MainViewModel ?? this.Parent?.MainViewModel;
            if (mainVM != null)
            {
                var pathsToDelete = IsFolder
                    ? GetAllDescendantPaths(this).ToHashSet(StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string> { Path };

                var docsToClose = mainVM.OpenDocuments
                    .Where(od => od.Document != null && pathsToDelete.Contains(od.Document.Path))
                    .ToList();

                if (docsToClose.Count > 0)
                {
                    Console.WriteLine($"[Delete] 需要关闭 {docsToClose.Count} 个打开的文档。");
                    foreach (var od in docsToClose)
                    {
                        string docPath = od.Document != null ? od.Document.Path : "(null)";
                        string docTitle = od.Title ?? "(无标题)";
                        Console.WriteLine($"[Delete] 关闭文档: 标题={docTitle}, 路径={docPath}");

                        // 安全关闭：捕获异常不影响后续删除流程
                        try
                        {
                            mainVM.CloseDocument(od).Wait();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Delete] 关闭文档异常: {ex.Message} (路径={docPath})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[Delete] 未找到需要关闭的打开文档（当前路径: {Path}）。");
                }
            }
            else
            {
                Console.WriteLine("[Delete] MainViewModel 未初始化，无法关闭打开文档。");
                Console.WriteLine($"[Delete] 当前节点: {Name}, 路径: {Path}。");
            }

            // 2. 删除磁盘文件/文件夹
            if (IsFolder)
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                    Console.WriteLine($"[Delete] 已删除文件夹: {Path}");
                }
                else
                {
                    Console.WriteLine($"[Delete] 文件夹不存在: {Path}");
                }
            }
            else
            {
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                    Console.WriteLine($"[Delete] 已删除文件: {Path}");
                }
                else
                {
                    Console.WriteLine($"[Delete] 文件不存在: {Path}");
                }
            }

            // 3. 从父节点移并刷新父节点可见性/可见子集合
            if (Parent?.Children != null)
            {
                Parent.Children.Remove(this);
                Parent.OnPropertyChanged(nameof(Parent.VisibleChildren));
                Parent.OnPropertyChanged(nameof(Parent.IsVisible));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Delete] 删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前节点及其所有后代节点的路径（深度优先）。
    /// </summary>
    private static IEnumerable<string> GetAllDescendantPaths(DocumentViewModel node)
    {
        yield return node.Path;
        if (node.IsFolder && node.Children != null)
        {
            foreach (var child in node.Children)
            {
                foreach (var path in GetAllDescendantPaths(child))
                    yield return path;
            }
        }
    }

    private ICommand? _deleteCommand;

    /// <summary>
    /// 删除命令（文件/文件夹通用）。
    /// </summary>
    public ICommand DeleteFileCommand => _deleteCommand ??= new RelayCommand(_ => Delete());

    /// <summary>
    /// 仅修改显示名称（不涉及磁盘重命名）。
    /// <para>注意：这里只变更 VM 的 Name 属性，调用方需自行处理磁盘重命名与 Path 更新。</para>
    /// </summary>
    public void Rename(string newName)
    {
        Name = newName;
        OnPropertyChanged(nameof(Name));
    }

    private RoslynHost? _roslynHost;

    /// <summary>
    /// 关闭指定 OpenDocumentViewModel（直接通过 RoslynHost 关闭文档并调用 document.Close）。
    /// </summary>
    public void CloseDocument(OpenDocumentViewModel? document)
    {
        if (document == null)
        {
            return;
        }

        if (document.HasDocumentId)
        {
            RoslynHost?.CloseDocument(document.DocumentId);
        }

        //OpenDocuments.Remove(document);
        document.Close();
    }

    /// <summary>
    /// RoslynHost 引用（用于关闭 Roslyn 文档等）。
    /// </summary>
    public RoslynHost RoslynHost
    {
        get => _roslynHost.NotNull();
        private set => _roslynHost = value;
    }

    /// <summary>
    /// 通过 MainViewModel 关闭指定 OpenDocumentViewModel。
    /// </summary>
    public void CloseOpenDocument(OpenDocumentViewModel openDocument)
    {
        var mainVM = this.MainViewModel ?? this.Parent?.MainViewModel;
        if (mainVM != null && openDocument != null)
        {
            try
            {
                mainVM.CloseDocument(openDocument).Wait();
                Console.WriteLine($"[CloseOpenDocument] 已关闭文档: {openDocument.Title ?? "(无标题)"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CloseOpenDocument] 关闭文档异常: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[CloseOpenDocument] MainViewModel 未初始化或参数无效，无法关闭文档。");
        }
    }

    /// <summary>
    /// 确保当前节点及其子节点都持有同一个 MainViewModel 引用。
    /// <para>用于树初始化/挂载后补全上下文。</para>
    /// </summary>
    public void EnsureMainViewModel(MainViewModel mainVM)
    {
        if (MainViewModel == null) MainViewModel = mainVM;
        if (IsFolder && Children != null)
        {
            foreach (var child in Children)
            {
                child.EnsureMainViewModel(mainVM);
            }
        }
    }

    /// <summary>
    /// 本地化管理器（用于 XAML 绑定）。
    /// </summary>
    public LocalizationManager Localized => LocalizationManager.Instance;
}
