using Avalonia.Controls;
using AvaloniaEdit.Document;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Editor;
using RoslynPad.Build;
using RoslynPad.UI;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using DialogHostAvalonia;
using RoslynPad.ViewModels;

namespace RoslynPad;

/// <summary>
/// 文档编辑视图（Avalonia）：承载 <see cref="RoslynCodeEditor"/>，
/// 负责与 <see cref="OpenDocumentViewModel"/> 建立双向联动：
/// - 文本加载与 Roslyn 初始化；
/// - 搜索高亮/跳转/清除；
/// - NuGet 安装自动注入引用行；
/// - 主题与字体联动；
/// - 控制台输入弹窗与回传。
/// </summary>
/// <remarks>
/// 生命周期：在 <see cref="OnDataContextChanged(object?, EventArgs)"/> 里完成一次性绑定与初始化；
/// 主题/字体变化通过 VM 事件联动更新；关闭/销毁走 <see cref="Dispose"/>（当前实现为空占位）。<br/>
/// 线程模型：涉及 UI 的更新统一封送回 UI 线程（例如 NuGet 回调中的 <c>GetDispatcher().InvokeAsync</c>）。
/// </remarks>
partial class DocmentMainView : UserControl, IDisposable
{
    /// <summary>实际承载代码编辑功能的控件（来自 XAML 名为 "Editor" 的 <see cref="RoslynCodeEditor"/>）。</summary>
    private readonly RoslynCodeEditor _editor;

    /// <summary>当前绑定的文档 VM；在 <see cref="OnDataContextChanged(object?, EventArgs)"/> 中赋值。</summary>
    private OpenDocumentViewModel? _viewModel;

    /// <summary>
    /// 构造并查找编辑器控件，订阅 DataContext 变化事件。
    /// </summary>
    /// <exception cref="InvalidOperationException">当找不到名为 "Editor" 的控件时抛出。</exception>
    public DocmentMainView()
    {
        InitializeComponent();

        // 必须存在的编辑器控件
        _editor = this.FindControl<RoslynCodeEditor>("Editor") ?? throw new InvalidOperationException("Missing Editor");

        // DataContext 变更时初始化/解绑
        DataContextChanged += OnDataContextChanged;

        this.DataContextChanged += DocmentMainView_DataContextChanged;
    }

    /// <summary>
    /// 强类型访问当前的 <see cref="OpenDocumentViewModel"/>；若为空则抛异常（用于绑定期望）。
    /// </summary>
    public OpenDocumentViewModel ViewModel => _viewModel.NotNull();

    /// <summary>
    /// 当 DataContext 切换为 <see cref="OpenDocumentViewModel"/> 时，完成编辑器与 VM 的初始化与事件联动。
    /// </summary>
    private async void OnDataContextChanged(object? sender, EventArgs args)
    {
        if (DataContext is not OpenDocumentViewModel viewModel) return;
        _viewModel = viewModel;

        // —— 搜索相关：高亮/跳转/清除 —— //
        viewModel.SearchHighlightRequested += (spans, currentIndex) =>
        {
            _editor.HighlightSpans(spans, currentIndex);
        };

        viewModel.SearchJumpRequested += span =>
        {
            _editor.JumpToSpan(span);
        };

        viewModel.SearchClearHighlightRequested += () =>
        {
            _editor.ClearSearchHighlight();
        };

        // —— 包管理与交互事件 —— //
        viewModel.NuGet.PackageInstalled += NuGetOnPackageInstalled;

        viewModel.ReadInput += OnReadInput;                  // 控制台输入弹窗
        viewModel.EditorFocus += (o, e) => _editor.Focus();  // 请求聚焦到编辑器

        // —— 主题与字体联动 —— //
        viewModel.MainViewModel.EditorFontSizeChanged += size => _editor.FontSize = size;
        viewModel.MainViewModel.ThemeChanged += OnThemeChanged;
        _editor.FontSize = viewModel.MainViewModel.EditorFontSize;
        SetFontFamily(); // 根据 Settings.EditorFontFamily 逗号分隔表，逐个尝试解析

        // —— 文本加载 & Roslyn 初始化 —— //
        var documentText = await viewModel.LoadTextAsync().ConfigureAwait(true);

        var documentId = await _editor.InitializeAsync(
            viewModel.MainViewModel.RoslynHost,
            new ThemeClassificationColors(viewModel.MainViewModel.Theme),
            viewModel.WorkingDirectory,
            documentText,
            viewModel.SourceCodeKind
        ).ConfigureAwait(true);

        // 告知 VM：如何获取选择区间、如何回传错误、以及该视图实例
        viewModel.Initialize(
            documentId,
            OnError,
            () => new TextSpan(_editor.SelectionStart, _editor.SelectionLength),
            this);

        // 文本改变 -> 通知 VM（脏标记/标题*等由 VM 决定）
        _editor.Document.TextChanged += (o, e) => viewModel.OnTextChanged();

        // 本地函数：设置编辑器字体（按优先级尝试）
        void SetFontFamily()
        {
            var fonts = viewModel.MainViewModel.Settings.EditorFontFamily.Split(',');
            foreach (var font in fonts)
            {
                try
                {
                    _editor.FontFamily = FontFamily.Parse(font);
                    break; // 成功解析并应用后退出
                }
                catch
                {
                    // 忽略解析失败，继续尝试下一个字体
                }
            }
        }
    }

    /// <summary>
    /// 弹出一个简单的输入对话框用于“控制台输入”，并在关闭时将文本回传给 VM。
    /// </summary>
    private async void OnReadInput()
    {
        var textBox = new TextBox();

        var dialog = new HeaderedContentControl
        {
            Header = "Console Input",
            Content = textBox,
            Background = Brushes.White,
        };

        // 自动聚焦到文本框
        textBox.Loaded += (o, e) => textBox.Focus();

        // Enter 关闭对话框
        textBox.KeyDown += (o, e) =>
        {
            if (e.Key == Key.Enter)
            {
                DialogHost.Close(MainWindow.DialogHostIdentifier);
            }
        };

        // 展示模态对话框（使用全局 DialogHost 标识）
        await DialogHost.Show(dialog, MainWindow.DialogHostIdentifier).ConfigureAwait(true);

        // 将用户输入回传给 VM
        ViewModel.SendInput(textBox.Text ?? string.Empty);
    }

    /// <summary>
    /// 主题变更时刷新编辑器的分类配色（语法高亮方案）。
    /// </summary>
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Editor.ClassificationHighlightColors = new ThemeClassificationColors(ViewModel.MainViewModel.Theme);
    }

    /// <summary>
    /// NuGet 包安装成功后，在文档开头插入 <c>#r "nuget: 包, 版本"</c> 以便脚本引用。
    /// </summary>
    private void NuGetOnPackageInstalled(PackageData package)
    {
        // 派发到 UI 线程进行文档插入，避免跨线程访问编辑器
        _ = this.GetDispatcher().InvokeAsync(() =>
        {
            var text = $"#r \"nuget: {package.Id}, {package.Version}\"{Environment.NewLine}";
            _editor.Document.Insert(0, text, AnchorMovementType.Default);
        });
    }

    /// <summary>
    /// 执行/编译错误回调（当前占位：可在此统一弹框、状态栏提示或错误面板输出）。
    /// </summary>
    private void OnError(ExceptionResultObject? e)
    {
        // 预留：根据需要把错误展示到输出面板/状态栏
    }

    /// <summary>
    /// 释放资源与事件解绑（当前实现为空，占位供后续扩展）。
    /// </summary>
    public void Dispose()
    {
        // 建议：如后续增加事件订阅（尤其是 VM 级别），在此统一退订，避免内存泄漏
        // e.g. _viewModel!.NuGet.PackageInstalled -= NuGetOnPackageInstalled;
    }

    /// <summary>
    /// 另一处 DataContext 变化处理：当变为 null 时清空编辑器并设为只读
    /// </summary>
    private void DocmentMainView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext == null)
        {
            // 假设有一个编辑器控件名为 Editor（来自 XAML）
            Editor.Text = string.Empty;
            Editor.IsReadOnly = true; // 可选：设为只读
            if (Editor.Text != null)
            {
                Editor.Text = null;
            }
        }
        else
        {
            // 可选：恢复只读状态或加载新内容（如有需要）
        }
    }
}
