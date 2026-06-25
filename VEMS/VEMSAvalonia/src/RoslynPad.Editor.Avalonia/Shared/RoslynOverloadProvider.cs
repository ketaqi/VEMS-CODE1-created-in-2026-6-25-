using RoslynPad.Roslyn;
using RoslynPad.Roslyn.SignatureHelp;

namespace RoslynPad.Editor;

/// <summary>
/// Roslyn 方法重载提供器，用于显示方法签名帮助。
/// </summary>
/// <remarks>
/// 当用户输入方法调用时，此类负责显示方法的各个重载版本，
/// 包括参数信息和文档注释。
/// </remarks>
internal sealed class RoslynOverloadProvider : NotificationObject, IOverloadProviderEx
{
    private readonly SignatureHelpItems _signatureHelp;
    private readonly IList<SignatureHelpItem> _items;

    private int _selectedIndex;
    private SignatureHelpItem? _item;
    private object? _currentHeader;
    private object? _currentContent;
    private string? _currentIndexText;

    /// <summary>
    /// 初始化 <see cref="RoslynOverloadProvider"/> 类的新实例。
    /// </summary>
    /// <param name="signatureHelp">签名帮助项目集合。</param>
    public RoslynOverloadProvider(SignatureHelpItems signatureHelp)
    {
        _signatureHelp = signatureHelp;
        _items = signatureHelp.Items;
        if (signatureHelp.SelectedItemIndex != null)
        {
            _selectedIndex = signatureHelp.SelectedItemIndex.Value;
        }
    }

    /// <summary>
    /// 获取或设置当前选中的重载索引。
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (SetProperty(ref _selectedIndex, value))
            {
                Refresh();
            }
        }
    }

    /// <summary>
    /// 刷新显示内容。
    /// </summary>
    public void Refresh()
    {
        _item = _items[_selectedIndex];
        var headerPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                _item.PrefixDisplayParts.ToTextBlock(),
            }
        };
        var contentPanel = new StackPanel();

        var docText = _item.DocumentationFactory(CancellationToken.None).ToTextBlock();
        if (HasContent(docText))
        {
            contentPanel.Children.Add(docText);
        }
        if (!_item.Parameters.IsDefault)
        {
            for (var index = 0; index < _item.Parameters.Length; index++)
            {
                var param = _item.Parameters[index];
                AddParameterSignatureHelp(_item, index, param, headerPanel, contentPanel);
            }
        }
        headerPanel.Children.Add(_item.SuffixDisplayParts.ToTextBlock());
        CurrentHeader = headerPanel;
        CurrentContent = contentPanel;
        CurrentIndexText = $" {_selectedIndex + 1} of {_items.Count} ";
    }

#if AVALONIA
    /// <summary>
    /// 检查面板是否有内容。
    /// </summary>
    private bool HasContent(Panel textBlock) => textBlock?.Children.Count > 0;
#else
    /// <summary>
    /// 检查 TextBlock 是否有内容。
    /// </summary>
    private bool HasContent(TextBlock textBlock) => textBlock?.Inlines.Count > 0;
#endif

    /// <summary>
    /// 添加参数的签名帮助信息。
    /// </summary>
    private void AddParameterSignatureHelp(SignatureHelpItem item, int index, SignatureHelpParameter param, Panel headerPanel, Panel contentPanel)
    {
        var isSelected = _signatureHelp.SelectedItemIndex == index;
        headerPanel.Children.Add(param.DisplayParts.ToTextBlock(isBold: isSelected));
        if (index != item.Parameters.Length - 1)
        {
            headerPanel.Children.Add(item.SeparatorDisplayParts.ToTextBlock());
        }
        if (isSelected)
        {
            var textBlock = param.DocumentationFactory(CancellationToken.None).ToTextBlock();
            if (HasContent(textBlock))
            {
                contentPanel.Children.Add(new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock { Text = param.Name + ": ", FontWeight = CommonFontWeights.Bold },
                        textBlock
                    }
                });
            }
        }
    }

    /// <summary>
    /// 获取重载数量。
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// 获取当前索引文本（如 "1 of 3"）。
    /// </summary>
    public string? CurrentIndexText
    {
        get => _currentIndexText;
        private set => SetProperty(ref _currentIndexText, value);
    }

    /// <summary>
    /// 获取当前标题（方法签名）。
    /// </summary>
    public object? CurrentHeader
    {
        get => _currentHeader;
        private set => SetProperty(ref _currentHeader, value);
    }

    /// <summary>
    /// 获取当前内容（文档和参数说明）。
    /// </summary>
    public object? CurrentContent
    {
        get => _currentContent;
        private set => SetProperty(ref _currentContent, value);
    }
}
