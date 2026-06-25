using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using RoslynPad.UI;
using Avalonia;

namespace RoslynPad;

partial class ResultsView : UserControl
{
    public ResultsView()
    {
        InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;

        // 可选：注册右键菜单可用性刷新
        if (OutputBox1.ContextMenu is ContextMenu menu)
        {
            menu.Opened += OutputBox1_ContextMenu_Opened;
        }
        // 监听 Text 变化实现自动下拉
        ((AvaloniaObject)OutputBox1)
        .GetObservable(TextBox.TextProperty)
        .Subscribe(_ => ScrollToEndIfNeeded());
    }

    private void ScrollToEndIfNeeded()
    {
        // 简单实现: 总是下拉到底部
        if (OutputBox1.Text != null)
        {
            OutputBox1.CaretIndex = OutputBox1.Text.Length;
            OutputBox1.BringIntoView();
        }
    }

    private IResultsViewModel? _currentVm;

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        //if (DataContext is not OpenDocumentViewModel viewModel) return;
        //void SetFontFamily()
        //{
        //    var fontObj = viewModel.MainViewModel.SelectedFontFamilyObject1;
        //    if (fontObj != null)
        //    {
        //        OutputBox1.FontFamily = fontObj;
        //    }
        //    else
        //    {
        //        // 兜底用字符串
        //        var fontName = viewModel.MainViewModel.EditorFontFamily1;

        //    }
        //}
        //// 取消之前的订阅，避免内存泄漏
        //if (_currentVm != null)
        //{
        //    _currentVm.PropertyChanged -= Vm_PropertyChanged;
        //    _currentVm = null;
        //}

        //if (DataContext is OpenDocumentViewModel vm)
        //{
        //    _currentVm = vm;
        //    _currentVm.PropertyChanged += Vm_PropertyChanged;

        //    // 首次刷新
        //    OutputBox1.FontFamily = vm.EditorFontFamily1;
        //    //OutputBox.FontSize = vm.FontSize;
        //    Console.WriteLine($"[OnDataContextChanged] OutputBoxC.FontSize已刷新: {vm.FontSize}");
        //    Console.WriteLine($"[OnDataContextChanged] OutputBoxC.FontFamily已刷新: {vm.EditorFontFamily1}");
        //}
        //else
        //{
        //    Console.WriteLine("[OnDataContextChanged] DataContext不是OpenDocumentViewModel");
        //}
        //SetFontFamily();
        // 取消之前的订阅
        if (_currentVm is INotifyPropertyChanged npcOld)
            npcOld.PropertyChanged -= Vm_PropertyChanged;

        // 新 DataContext
        if (DataContext is IResultsViewModel vm && DataContext is INotifyPropertyChanged npc)
        {
            _currentVm = vm;
            npc.PropertyChanged += Vm_PropertyChanged;

            // 首次刷新
            TrySetFontFamily(vm.EditorFontFamily1);
        }
        else
        {
            _currentVm = null;
        }
    }
    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 响应任意 IResultsViewModel.EditorFontFamily1 变化
        if (e.PropertyName == nameof(IResultsViewModel.EditorFontFamily1) && sender is IResultsViewModel vm)
        {
            TrySetFontFamily(vm.EditorFontFamily1);
        }
    }

    private void TrySetFontFamily(string font)
    {
        OutputBox1.FontFamily = font;
        // 可加日志
        // Console.WriteLine($"[FontFamilyChanged] OutputBox1.FontFamily已刷新: {font}");
    }

    //private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(OpenDocumentViewModel.EditorFontFamily1) && sender is OpenDocumentViewModel vm)
    //    {
    //        OutputBox1.FontFamily = vm.EditorFontFamily1;
    //        Console.WriteLine($"[Vm_PropertyChanged] OutputBoxC.FontFamily已刷新: {vm.EditorFontFamily1}");
    //    }
    //}

    // 全选
    private void OutputBox1_SelectAll_Click(object? sender, RoutedEventArgs e)
    {
        OutputBox1.Focus();
        OutputBox1.SelectAll();
    }

    // 复制
    private void OutputBox1_Copy_Click(object? sender, RoutedEventArgs e)
    {
        OutputBox1.Focus();
        OutputBox1.Copy();
    }

    // 剪切
    private void OutputBox1_Cut_Click(object? sender, RoutedEventArgs e)
    {
        OutputBox1.Focus();
        OutputBox1.Cut();
    }

    // 粘贴
    private void OutputBox1_Paste_Click(object? sender, RoutedEventArgs e)
    {
        OutputBox1.Focus();
        OutputBox1.Paste();
    }

    // 可选：动态刷新菜单项可用性（按选区和只读状态）
    private async void OutputBox1_ContextMenu_Opened(object? sender, EventArgs e)
    {
        var menu = sender as ContextMenu;
        if (menu == null) return;

        bool hasSelection = !string.IsNullOrEmpty(OutputBox1.SelectedText);
        bool isReadOnly = OutputBox1.IsReadOnly;

        // Avalonia Clipboard API
        bool canPaste = !isReadOnly;
        var top = TopLevel.GetTopLevel(this);
        if (top?.Clipboard != null)
        {
            var text = await top.Clipboard.GetTextAsync().ConfigureAwait(true);
            canPaste = canPaste && !string.IsNullOrEmpty(text);
        }

        if (menu.Items[0] is MenuItem selectAllItem)
            selectAllItem.IsEnabled = true;
        if (menu.Items[1] is MenuItem copyItem)
            copyItem.IsEnabled = hasSelection;
        if (menu.Items[2] is MenuItem cutItem)
            cutItem.IsEnabled = hasSelection && !isReadOnly;
        if (menu.Items[3] is MenuItem pasteItem)
            pasteItem.IsEnabled = canPaste;
    }
}
