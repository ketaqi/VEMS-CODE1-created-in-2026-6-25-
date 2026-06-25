using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using RoslynPad.UI;
using Avalonia.VisualTree;
using System.Text;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using AvaloniaEdit.Document;
using RoslynPad.Editor;
using System.ComponentModel;

namespace RoslynPad;

/// <summary>
/// DLL扩展视图：
/// 负责NuGet包管理和本地DLL导入功能。
/// 提供文件选择、路径显示、DLL导入等交互功能。
/// </summary>
/// <remarks>
/// 本地DLL导入流程：
/// 1. 用户点击Browse按钮选择DLL文件
/// 2. 路径显示在TextBox中
/// 3. 用户点击Import按钮
/// 4. 生成 #r "path/to/dll" 引用行
/// 5. 插入到当前文档编辑器开头
/// </remarks>
public partial class DLLExpandView : UserControl, INotifyPropertyChanged
{
    //静态只读数组，优化性能
    private static readonly FilePickerFileType[] s_dllFileTypes =
    {
        new FilePickerFileType("Dynamic Link Library")
        {
            Patterns = ["*.dll"],
            AppleUniformTypeIdentifiers = ["public.data"],
            MimeTypes = ["application/octet-stream"]
        },
        new FilePickerFileType("All Files")
        {
            Patterns = ["*.*"]
        }
    };

    private string _selectedDllPath = string.Empty;
    private string _dllImportStatus = string.Empty;
    private IBrush _dllImportStatusColor = Brushes.Black;

    public DLLExpandView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region 本地DLL导入相关属性

    /// <summary>选中的DLL文件路径</summary>
    public string SelectedDllPath
    {
        get => _selectedDllPath;
        set
        {
            if (_selectedDllPath != value)
            {
                _selectedDllPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanImportDll));
            }
        }
    }

    /// <summary>DLL导入状态信息</summary>
    public string DllImportStatus
    {
        get => _dllImportStatus;
        set
        {
            if (_dllImportStatus != value)
            {
                _dllImportStatus = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>DLL导入状态文字颜色</summary>
    public IBrush DllImportStatusColor
    {
        get => _dllImportStatusColor;
        set
        {
            if (_dllImportStatusColor != value)
            {
                _dllImportStatusColor = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>是否可以导入DLL（有有效路径时为true）</summary>
    public bool CanImportDll => !string.IsNullOrWhiteSpace(SelectedDllPath) && File.Exists(SelectedDllPath);

    #endregion

    #region 事件处理

    /// <summary>选择DLL文件按钮点击事件</summary>
    private async void SelectDllButton_Click(object? sender, RoutedEventArgs e)
    {
        await SelectDllFileAsync().ConfigureAwait(true);
    }

    /// <summary>导入DLL按钮点击事件</summary>
    private async void ImportDllButton_Click(object? sender, RoutedEventArgs e)
    {
        await ImportDllAsync().ConfigureAwait(true);
    }

    #endregion

    #region 本地DLL导入功能实现

    /// <summary>
    /// 打开文件选择对话框，让用户选择DLL文件
    /// </summary>
    private async Task SelectDllFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
            {
                UpdateImportStatus("Error: Cannot access file system", Brushes.Red);
                return;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select DLL File",
                AllowMultiple = false,
                FileTypeFilter = s_dllFileTypes
            }).ConfigureAwait(true);

            if (files.Count > 0)
            {
                var selectedFile = files[0];
                if (selectedFile.TryGetLocalPath() is string localPath)
                {
                    SelectedDllPath = localPath;
                    UpdateImportStatus($"Selected: {Path.GetFileName(localPath)}", Brushes.Green);
                }
                else
                {
                    UpdateImportStatus("Error: Cannot access selected file", Brushes.Red);
                }
            }
        }
        catch (Exception ex)
        {
            UpdateImportStatus($"Error selecting file: {ex.Message}", Brushes.Red);
        }
    }

    /// <summary>
    /// 导入选中的DLL文件到当前文档
    /// </summary>
    private async Task ImportDllAsync()
    {
        try
        {
            if (!CanImportDll)
            {
                UpdateImportStatus("Error: No valid DLL file selected", Brushes.Red);
                return;
            }

            // 获取当前文档的编辑器 - 使用多种方法尝试
            var editor = GetCurrentEditor();
            if (editor == null)
            {
                UpdateImportStatus("Error: No active editor found", Brushes.Red);
                return;
            }

            // 生成DLL引用行，格式与NuGet保持一致
            var referenceText = GenerateDllReferenceText(SelectedDllPath);

            // 插入到文档开头（模仿NuGet包导入的行为）
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                editor.Document.Insert(0, referenceText, AnchorMovementType.Default);
            });

            // 更新状态
            var fileName = Path.GetFileName(SelectedDllPath);
            UpdateImportStatus($"Successfully imported: {fileName}", Brushes.Green);

            // 清空路径，为下次导入做准备
            await Task.Delay(2000).ConfigureAwait(false); // 显示成功信息2秒
            SelectedDllPath = string.Empty;
            UpdateImportStatus(string.Empty, Brushes.Black);
        }
        catch (Exception ex)
        {
            UpdateImportStatus($"Error importing DLL: {ex.Message}", Brushes.Red);
        }
    }

    /// <summary>
    /// 生成DLL引用文本，格式类似于NuGet的 #r "nuget: package, version"
    /// </summary>
    /// <param name="dllPath">DLL文件路径</param>
    /// <returns>引用文本行</returns>
    private static string GenerateDllReferenceText(string dllPath)
    {
        // 使用绝对路径确保引用正确
        var absolutePath = Path.GetFullPath(dllPath);

        // 转换为正斜杠，避免转义问题
        var normalizedPath = absolutePath.Replace('\\', '/');

        // 生成引用行，格式：#r "path/to/dll"
        return $"#r \"{normalizedPath}\"{Environment.NewLine}";
    }

    /// <summary>
    /// 获取当前活动的代码编辑器实例 - 改进版本，使用多种策略查找
    /// </summary>
    /// <returns>编辑器实例，如果没有找到则返回null</returns>
    private RoslynCodeEditor? GetCurrentEditor()
    {
        try
        {
            // 策略1：通过MainWindow查找当前活动的编辑器
            var mainWindow = this.FindAncestorOfType<Window>();
            if (mainWindow?.DataContext is UI.MainViewModel mainViewModel)
            {
                var currentDoc = mainViewModel.CurrentOpenDocument;
                if (currentDoc?.EditorControl != null)
                {
                    UpdateImportStatus($"Found editor via MainViewModel: {currentDoc.Title}", Brushes.Blue);
                    return currentDoc.EditorControl;
                }
            }

            // 策略2：从可视树中查找所有RoslynCodeEditor，返回可见且聚焦的
            var editors = mainWindow?.GetVisualDescendants()
                .OfType<RoslynCodeEditor>()
                .Where(e => e.IsVisible && e.IsEffectivelyEnabled)
                .ToList();

            if (editors?.Count > 0)
            {
                // 优先返回有焦点的编辑器
                var focusedEditor = editors.FirstOrDefault(e => e.IsFocused);
                if (focusedEditor != null)
                {
                    UpdateImportStatus("Found focused editor", Brushes.Blue);
                    return focusedEditor;
                }

                // 如果没有焦点的，返回第一个可见的
                UpdateImportStatus($"Found {editors.Count} visible editors, using first one", Brushes.Blue);
                return editors.First();
            }

            // 策略3：通过名称查找（兜底方案）
            var editorByName = mainWindow?.FindControl<RoslynCodeEditor>("Editor");
            if (editorByName != null)
            {
                UpdateImportStatus("Found editor by name 'Editor'", Brushes.Blue);
                return editorByName;
            }

            // 策略4：通过DataContext链查找
            var parent = this.FindAncestorOfType<ContentControl>();
            while (parent != null)
            {
                if (parent.DataContext is UI.OpenDocumentViewModel documentViewModel)
                {
                    var editor = documentViewModel.EditorControl;
                    if (editor != null)
                    {
                        UpdateImportStatus($"Found editor via DataContext: {documentViewModel.Title}", Brushes.Blue);
                        return editor;
                    }
                }
                parent = parent.FindAncestorOfType<ContentControl>();
            }

            UpdateImportStatus("Debug: No editor found with any strategy", Brushes.Orange);
            return null;
        }
        catch (Exception ex)
        {
            UpdateImportStatus($"Error finding editor: {ex.Message}", Brushes.Red);
            return null;
        }
    }

    /// <summary>
    /// 更新DLL导入状态显示
    /// </summary>
    /// <param name="message">状态消息</param>
    /// <param name="color">消息颜色</param>
    private void UpdateImportStatus(string message, IBrush color)
    {
        DllImportStatus = message;
        DllImportStatusColor = color;
    }

    #endregion
}
