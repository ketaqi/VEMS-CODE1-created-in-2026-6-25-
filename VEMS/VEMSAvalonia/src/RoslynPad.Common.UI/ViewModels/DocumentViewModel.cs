using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Avalonia.Controls.ApplicationLifetimes;
using System.Windows.Input;
using RoslynPad.Build;
using RoslynPad.UI.ViewModels;
using Avalonia;
using System.Collections.Generic;
using System.Linq;
using System;
using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using RoslynPad.UI.Dialogs;
using RoslynPad.Roslyn;

namespace RoslynPad.UI;

[DebuggerDisplay("{Name}:{IsFolder}")]
public partial class DocumentViewModel : NotificationObject
{
    internal const string AutoSaveSuffix = ".autosave";

    //public static ImmutableArray<string> RelevantFileExtensions { get; } = [".cs", ".csx"];
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

    // ★ 新增：父节点引用（用于向上刷新可见性）
    public DocumentViewModel? Parent { get; private set; }

    private DocumentViewModel(string rootPath, bool isFolder)
    {
        Path = rootPath;
        IsFolder = isFolder;

        var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(Name);
        IsAutoSave = nameWithoutExtension.EndsWith(AutoSaveSuffix, StringComparison.OrdinalIgnoreCase);
        if (IsAutoSave)
        {
            Name = string.Concat(nameWithoutExtension.AsSpan(0, nameWithoutExtension.Length - AutoSaveSuffix.Length), System.IO.Path.GetExtension(Name));
        }

        IsSearchMatch = true;
    }

    public void ChangePath(string newPath)
    {
        Path = newPath;
        _orderByName = null;
    }

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

    public bool IsFolder { get; }

    public string GetSavePath()
    {
        return IsAutoSave
            ? System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, Name)
            : Path;
    }

    public string GetAutoSavePath()
    {
        return IsAutoSave ?
            Path
            : System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, GetAutoSaveName(Name));
    }

    public static string GetAutoSaveName(string name)
    {
        return System.IO.Path.ChangeExtension(name, AutoSaveSuffix + System.IO.Path.GetExtension(name));
    }

    public static DocumentViewModel CreateRoot(string rootPath)
    {
        IOUtilities.PerformIO(() => Directory.CreateDirectory(rootPath));
        return new DocumentViewModel(rootPath, true);
    }

    public static DocumentViewModel FromPath(string path)
    {
        return new DocumentViewModel(path, isFolder: Directory.Exists(path));
    }

    public DocumentViewModel CreateNew(string documentName)
    {
        if (!IsFolder) throw new InvalidOperationException("Parent must be a folder");

        var document = new DocumentViewModel(GetDocumentPathFromName(Path, documentName), false)
        {
            Parent = this // ★ 维护父子关系
        };
        AddChild(document);
        return document;
    }

    public static string GetDocumentPathFromName(string path, string name) =>
        System.IO.Path.Combine(path, name);

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

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public string Name
    {
        get => _name.NotNull();
        private set => SetProperty(ref _name, value);
    }

    public bool IsAutoSave { get; }

    public bool IsAutoSaveOnly =>
        _isAutoSaveOnly ??= IsAutoSave &&
            !File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, Name));

    [MemberNotNullWhen(true, nameof(InternalChildren))]
    public bool IsChildrenInitialized => InternalChildren != null;

    internal DocumentCollection? InternalChildren { get; private set; }

    public ObservableCollection<DocumentViewModel>? Children
    {
        get
        {
            if (IsFolder && InternalChildren == null)
            {
                InternalChildren = ReadChildren();

                // ★ 初次读取时补建父子关系
                foreach (var c in InternalChildren)
                    c.Parent = this;
            }

            return InternalChildren;
        }
    }

    // —— 搜索匹配状态（你现有搜索逻辑会设置这个值）
    public bool IsSearchMatch
    {
        get => _isSearchMatch;
        internal set
        {
            if (SetProperty(ref _isSearchMatch, value))
            {
                // ★ 匹配状态变化 => 刷新自身与祖先/子孙的可见性绑定
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(VisibleChildren));
                Parent?.RefreshSearchVisibilityUpward();
                if (IsFolder && IsChildrenInitialized)
                {
                    foreach (var c in InternalChildren!)
                        c.RefreshSearchVisibility();
                }
            }
        }
    }

    // ★ 新增：基于搜索的“是否可见”规则 = 自己命中 或 有可见子孙
    public bool IsVisible =>
        IsSearchMatch || (IsFolder && (Children?.Any(c => c.IsVisible) ?? false));

    // ★ 新增：只暴露“可见的”子项（TreeView 绑定它来实现过滤显示）
    public IEnumerable<DocumentViewModel> VisibleChildren =>
        (Children ?? Enumerable.Empty<DocumentViewModel>()).Where(c => c.IsVisible);

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

        // 排序并打包
        var collection = new DocumentCollection(directories.Concat(files.OrderBy(file => file.OrderByName)));

        // ★ 维护父引用
        foreach (var c in collection)
            c.Parent = this;

        return collection;
    }

    private string OrderByName =>
        _orderByName ??= NumberRegex().Replace(Name, m => m.Value.PadLeft(100, '0'));

    internal void AddChild(DocumentViewModel documentViewModel)
    {
        ArgumentNullException.ThrowIfNull(documentViewModel);

        // 确保集合初始化
        if (InternalChildren is null)
        {
            _ = Children; // 触发惰性加载
            InternalChildren ??= new DocumentCollection(Enumerable.Empty<DocumentViewModel>());
        }

        var children = InternalChildren;

        // 已有同名：不重复插入，只同步父/VM并刷新可见
        var existing = children[documentViewModel.Name];
        if (existing is not null)
        {
            existing.Parent = this;
            existing.MainViewModel ??= this.MainViewModel ?? this.Parent?.MainViewModel; // ★ 传递 MainViewModel
            IsExpanded = true;
            OnPropertyChanged(nameof(VisibleChildren));
            return;
        }

        // 插入位置（文件夹优先，同类按名称升序）
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

        documentViewModel.Parent = this;
        documentViewModel.MainViewModel ??= this.MainViewModel ?? this.Parent?.MainViewModel; // ★ 传递 MainViewModel
        children.Insert(insertIndex, documentViewModel);

        OnPropertyChanged(nameof(VisibleChildren));
        IsExpanded = true;

        ArgumentNullException.ThrowIfNull(documentViewModel);

        documentViewModel.MainViewModel ??= this.MainViewModel ?? this.Parent?.MainViewModel;
    }


    // ★ 新增：向下刷新（当某节点的匹配状态变化时）
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

    // ★ 新增：向上刷新（当某子节点匹配状态变化时）
    public void RefreshSearchVisibilityUpward()
    {
        OnPropertyChanged(nameof(IsVisible));
        OnPropertyChanged(nameof(VisibleChildren));
        Parent?.RefreshSearchVisibilityUpward();
    }

    [GeneratedRegex("[0-9]+")]
    private static partial Regex NumberRegex();

    // ===================== 你已有的自定义区（原样保留） =====================

    private ICommand? _copyFilePathCommand;
    public ICommand CopyFilePathCommand => _copyFilePathCommand ??= new RelayCommand(_ => CopyFilePath());
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
                    Console.WriteLine("未能获取剪贴板对象（Clipboard为null）");
                }
            }
            else
            {
                Console.WriteLine("未找到活动窗口，无法访问剪贴板");
            }
        }
    }

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

            // 跨平台打开文件夹
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
    public ICommand OpenContainingFolderCommand => _openContainingFolderCommand ??= new RelayCommand(_ => OpenContainingFolder());

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
                Console.WriteLine("[CopyTextToClipboard] 剪贴板内容: " + clipboardText);
            }
            else
            {
                Console.WriteLine("[CopyTextToClipboard] 未能获取剪贴板对象（Clipboard为null）");
            }
        }
        else
        {
            Console.WriteLine("[CopyTextToClipboard] 未找到活动窗口，无法访问剪贴板");
        }
    }

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
                Console.WriteLine("[CopyFolderPathToClipboard] 剪贴板内容: " + clipboardText);
            }
            else
            {
                Console.WriteLine("[CopyFolderPathToClipboard] 未能获取剪贴板对象（Clipboard为null）");
            }
        }
        else
        {
            Console.WriteLine("[CopyFolderPathToClipboard] 未找到活动窗口，无法访问剪贴板");
        }
    }

    private ICommand? _copyFolderPathCommand;

    public ICommand CopyFolderPathCommand => _copyFolderPathCommand ??= new RelayCommand(_ => CopyFolderPathToClipboard());

    //private ICommand? _createFileCommand;
    //public ICommand CreateFileCommand => _createFileCommand ??= new RelayCommand(_ =>
    //{
    //    CreateFile("新建文件.cs");
    //});

    private ICommand? _createFileCommand;

    public ICommand CreateFileCommand => _createFileCommand ??= new RelayCommand(async _ =>
    {
        if (!IsFolder)
        {
            MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
            {
                Header = "[警告]",
                Value = "只能在文件夹下新建文件",
                Type = "警告",
                LineNumber = null
            });
            return;
        }

        // 获取主窗口
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.Windows.FirstOrDefault(w => w.IsActive)
            : null;

        string? fileName = null;

        // 推荐使用 MsBox.Avalonia 或自定义输入对话框
        if (window != null)
        {
            var inputDialog = new InputDialog("新建文件", "请输入新文件名：", "新建文件.cs");
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
            MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
            {
                Header = "[警告]",
                Value = "文件名不能为空",
                Type = "警告",
                LineNumber = null
            });
            return;
        }

        // 检查是否已存在
        var filePath = GetDocumentPathFromName(Path, fileName);
        if (File.Exists(filePath))
        {
            MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
            {
                Header = "[警告]",
                Value = $"文件已存在: {fileName}",
                Type = "警告",
                LineNumber = null
            });
            return;
        }

        // 创建文件
        File.WriteAllText(filePath, string.Empty);
        var newFile = new DocumentViewModel(filePath, false) { Parent = this };
        AddChild(newFile);
        Console.WriteLine($"[DocumentViewModel] 新建文件: {filePath}");
    });

    public MainViewModel? MainViewModel { get; set; } // 需在外部赋值

    private string GetUniqueChildName(string baseName, bool isFolder)
    {
        // 改为 OrdinalIgnoreCase，满足 CA1309
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


    public void CreateFolder(string folderName)
    {
        if (!IsFolder)
        {
            MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
            {
                Header = "[警告]",
                Value = "只能在文件夹下新建文件夹",
                Type = "警告",
                LineNumber = null
            });
            return;
        }

        // 生成不重名的名称
        var uniqueName = GetUniqueChildName(folderName, isFolder: true);
        var folderPath = GetDocumentPathFromName(Path, uniqueName);

        Directory.CreateDirectory(folderPath);

        var newFolder = new DocumentViewModel(folderPath, true)
        {
            Parent = this,
            // 关键：把 MainViewModel 传下去（当前或向上取）
            MainViewModel = this.MainViewModel ?? this.Parent?.MainViewModel
        };

        AddChild(newFolder);

        // 进入重命名态
        newFolder.IsEditing = true;

        // 安全选中（可能为 null 就不选）
        var owner = this.MainViewModel ?? this.Parent?.MainViewModel;
        if (owner != null)
            owner.SelectedDocument = newFolder;

        Console.WriteLine($"[DocumentViewModel] 新建文件夹: {folderPath}");
    }


    public void CreateFile(string fileName)
    {
        if (!IsFolder)
        {
            MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
            {
                Header = "[警告]",
                Value = "只能在文件夹下新建文件",
                Type = "警告",
                LineNumber = null
            });
            return;
        }

        // ★ 生成不重名的名称
        var uniqueName = GetUniqueChildName(fileName, isFolder: false);
        var filePath = GetDocumentPathFromName(Path, uniqueName);

        File.WriteAllText(filePath, string.Empty);

        var newFile = new DocumentViewModel(filePath, false) { Parent = this };
        AddChild(newFile);

        // 可选：进入重命名态
        newFile.IsEditing = true;

        Console.WriteLine($"[DocumentViewModel] 新建文件: {filePath}");
    }


    private ICommand? _createFolderCommand;
    public ICommand CreateFolderCommand => _createFolderCommand ??= new RelayCommand(_ =>
    {
        CreateFolder("新建文件夹");
    });

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    //public void Delete()
    //{
    //    try
    //    {
    //        if (IsFolder)
    //        {
    //            if (Directory.Exists(Path))
    //            {
    //                Directory.Delete(Path, true);
    //                Console.WriteLine($"[DocumentViewModel] 已删除文件夹: {Path}");
    //            }
    //        }
    //        else
    //        {
    //            if (File.Exists(Path))
    //            {
    //                File.Delete(Path);
    //                Console.WriteLine($"[DocumentViewModel] 已删除文件: {Path}");
    //            }
    //        }

    //        // 从父节点移除自身（如果父引用已建立）
    //        if (Parent?.Children != null)
    //        {
    //            Parent.Children.Remove(this);
    //            // ★ 关键：刷新父节点的可见子项，确保界面更新
    //            Parent.OnPropertyChanged(nameof(Parent.VisibleChildren));
    //            Parent.OnPropertyChanged(nameof(Parent.IsVisible));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[DocumentViewModel] 删除失败: {ex.Message}");
    //    }
    //}


    //递归获取所有子孙节点路径（含自身）

    // 在 Delete 方法开头添加关闭打开文档的逻辑

    public void Delete()
    {
        try
        {
            // 1. 关闭所有相关打开文档
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
                    Console.WriteLine($"[Delete] 将要关闭 {docsToClose.Count} 个打开文档：");
                    foreach (var od in docsToClose)
                    {
                        string docPath = od.Document != null ? od.Document.Path : "(null)";
                        string docTitle = od.Title ?? "(无标题)";
                        Console.WriteLine($"[Delete] 关闭文档: 标题={docTitle}, 路径={docPath}");

                        // 安全调用关闭方法
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
                    Console.WriteLine($"[Delete] 未找到需要关闭的打开文档（路径: {Path}）");
                }
            }
            else
            {
                Console.WriteLine("[Delete] MainViewModel 未初始化，无法关闭打开文档");
                Console.WriteLine($"[Delete] MainViewModel 未初始化，无法关闭打开文档（当前节点: {Name}, 路径: {Path}）");
            }

            // 2. 删除实际文件或文件夹
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

            // 3. 移除节点
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
    public ICommand DeleteFileCommand => _deleteCommand ??= new RelayCommand(_ => Delete());

    public void Rename(string newName)
    {
        Name = newName;
        OnPropertyChanged(nameof(Name));
    }

    private RoslynHost? _roslynHost;

    public void  CloseDocument(OpenDocumentViewModel? document)
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
    public RoslynHost RoslynHost
    {
        get => _roslynHost.NotNull();
        private set => _roslynHost = value;
    }

    /// <summary>
    /// 通过 MainViewModel 关闭指定的 OpenDocumentViewModel 文档
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
            Console.WriteLine("[CloseOpenDocument] MainViewModel 未初始化或参数无效，无法关闭文档");
        }
    }
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
    //多语言使用的属性
    public LocalizationManager Localized => LocalizationManager.Instance;
}
