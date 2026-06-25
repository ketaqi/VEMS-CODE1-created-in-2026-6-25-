using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RoslynPad.UI;
using Avalonia.VisualTree;

namespace RoslynPad;

public partial class DocumentTreeView4 : UserControl
{
    private MainViewModel? _viewModel;

    public DocumentTreeView4()
    {
        InitializeComponent();
        var treeView = this.Find<TreeView>("Tree");
        if (treeView != null)
        {
            treeView.DoubleTapped += OnDocumentClick;
            treeView.KeyDown += OnDocumentKeyDown;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext != null)
            _viewModel = DataContext as MainViewModel;
    }

    private void OnDocumentClick(object? sender, RoutedEventArgs e)
    {
        OpenDocument(e.Source);
    }

    private void OnDocumentKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OpenDocument(e.Source);
        }
    }

    private void OpenDocument(object? source)
    {
        var item = (source as Visual)?.GetSelfAndVisualAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();

        if (item?.DataContext is DocumentViewModel documentViewModel)
        {
            _viewModel?.OpenDocument(documentViewModel);
        }
    }
}
