using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using RoslynPad.UI.ViewModels;
using System.Windows.Input;
using Avalonia;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using RoslynPad.Roslyn;
using RoslynPad.Build;
using RoslynPad.UI;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RoslynPad
{

        public partial class FolderViewModel : UI.NotificationObject
    {
        internal const string AutoSaveSuffix = ".autosave";

        public static ImmutableArray<string> RelevantFileExtensions { get; } = [".cs", ".csx"];

        private bool _isExpanded;
        private bool? _isAutoSaveOnly;
        private bool _isSearchMatch;
        private string? _path;
        private string? _name;
        private string? _orderByName;

        private FolderViewModel(string rootPath, bool isFolder)
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

            foreach (var child in InternalChildren)
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

        public static FolderViewModel CreateRoot(string rootPath)
        {
            IOUtilities.PerformIO(() => Directory.CreateDirectory(rootPath));
            return new FolderViewModel(rootPath, true);
        }

        public static FolderViewModel FromPath(string path)
        {
            return new FolderViewModel(path, isFolder: Directory.Exists(path));
        }

        public FolderViewModel CreateNew(string documentName)
        {
            if (!IsFolder) throw new InvalidOperationException("Parent must be a folder");

            var document = new FolderViewModel(GetDocumentPathFromName(Path, documentName), false);
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

        public ObservableCollection<FolderViewModel>? Children
        {
            get
            {
                if (IsFolder && InternalChildren == null)
                {
                    InternalChildren = ReadChildren();
                }

                return InternalChildren;
            }
        }

        public bool IsSearchMatch
        {
            get => _isSearchMatch;
            internal set => SetProperty(ref _isSearchMatch, value);
        }

        private DocumentCollection ReadChildren()
        {
            var directories =
                IOUtilities.EnumerateDirectories(Path)
                .Select(directory => new FolderViewModel(directory, isFolder: true))
                .OrderBy(directory => directory.OrderByName);

            var files = Enumerable.Empty<FolderViewModel>();

            foreach (var extension in RelevantFileExtensions)
            {
                files = files.Concat(IOUtilities.EnumerateFiles(Path, "*" + extension)
                    .Select(file => new FolderViewModel(file, isFolder: false))
                    .Where(file => !file.IsAutoSave));
            }

            return new DocumentCollection(directories.Concat(files.OrderBy(file => file.OrderByName)));
        }

        private string OrderByName =>
            _orderByName ??= NumberRegex().Replace(Name, m => m.Value.PadLeft(100, '0'));

        internal void AddChild(FolderViewModel documentViewModel)
        {
            var children = InternalChildren;
            if (children is null)
            {
                return;
            }

#pragma warning disable CA1309 // Use ordinal string comparison
            var insertIndex = children.IndexOf(f => f.IsFolder == documentViewModel.IsFolder &&
                                                    string.Compare(documentViewModel.OrderByName, f.OrderByName,
                                                        StringComparison.CurrentCulture) <= 0);
#pragma warning restore CA1309 // Use ordinal string comparison
            if (insertIndex < 0)
            {
                insertIndex = documentViewModel.IsFolder ? children.IndexOf(c => !c.IsFolder) : children.Count;

                if (insertIndex < 0)
                {
                    insertIndex = 0;
                }
            }

            children.Insert(insertIndex, documentViewModel);
        }

        [GeneratedRegex("[0-9]+")]
        private static partial Regex NumberRegex();

        //自定义

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
            // 获取文件夹路径（如果是文件则取其所在文件夹，如果是文件夹则取自身路径）
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
        public void CreateFile(string fileName)
        {

            if (!IsFolder)
            {
                Console.WriteLine("[测试] CreateFolder: 当前节点不是文件夹，已进入警告分支");
                // 输出警告到结果区
                MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
                {
                    Header = "[警告]",
                    Value = "只能在文件夹下新建文件夹",
                    Type = "警告",
                    LineNumber = null
                });
                return;
            }

            var filePath = GetDocumentPathFromName(Path, fileName);
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty); // 创建空文件
            }
            var newFile = new FolderViewModel(filePath, false);
            AddChild(newFile);
            Console.WriteLine($"[DocumentViewModel] 新建文件: {filePath}");
        }
        private ICommand? _createFileCommand;
        public ICommand CreateFileCommand => _createFileCommand ??= new RelayCommand(_ =>
        {
            // 这里可以弹窗让用户输入文件名，示例直接用默认名
            CreateFile("新建文件.cs");
        });
        public MainViewModel? MainViewModel { get; set; } // 需在外部赋值
        public void CreateFolder(string folderName)
        {
            if (!IsFolder)
            {
                Console.WriteLine("[测试] CreateFolder: 当前节点不是文件夹，已进入警告分支");
                // 输出警告到结果区
                Console.WriteLine($"[调试] CurrentOpenDocument: {(MainViewModel?.CurrentOpenDocument == null ? "null" : "已设置")}");

                MainViewModel?.CurrentOpenDocument?.AddResult(new ResultObject
                {
                    Header = "[警告]",
                    Value = "只能在文件夹下新建文件夹",
                    Type = "警告",
                    LineNumber = null
                });
                return;
            }

            var folderPath = GetDocumentPathFromName(Path, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);

            }
            var newFolder = new FolderViewModel(folderPath, true);
            AddChild(newFolder);
            Console.WriteLine($"[DocumentViewModel] 新建文件夹: {folderPath}");
        }
        private ICommand? _createFolderCommand;
        public ICommand CreateFolderCommand => _createFolderCommand ??= new RelayCommand(_ =>
        {
            // 这里可以弹窗让用户输入文件夹名，示例直接用默认名
            CreateFolder("新建文件夹");
        });
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
        public void Delete()
        {
            try
            {
                if (IsFolder)
                {
                    // 删除文件夹及其所有内容
                    if (Directory.Exists(Path))
                    {
                        Directory.Delete(Path, true);
                        Console.WriteLine($"[DocumentViewModel] 已删除文件夹: {Path}");
                    }
                }
                else
                {
                    if (File.Exists(Path))
                    {
                        File.Delete(Path);
                        Console.WriteLine($"[DocumentViewModel] 已删除文件: {Path}");
                    }
                }

                // 从父节点移除自身
                var parent = InternalChildren?.FirstOrDefault(c => c.Children?.Contains(this) == true);
                parent?.Children?.Remove(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DocumentViewModel] 删除失败: {ex.Message}");
            }
        }
        private ICommand? _deleteCommand;
        public ICommand DeleteFileCommand => _deleteCommand ??= new RelayCommand(_ => Delete());
        public void Rename(string newName)
        {
            Name = newName;
            OnPropertyChanged(nameof(Name));
        }
    }
}
