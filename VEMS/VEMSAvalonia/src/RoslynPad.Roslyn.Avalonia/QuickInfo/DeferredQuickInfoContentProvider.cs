using System.Composition;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn.Completion;

namespace RoslynPad.Roslyn.QuickInfo;

/// <summary>
/// 延迟加载的快速信息内容提供程序，实现<see cref="IDeferredQuickInfoContentProvider"/>接口
/// 负责创建快速信息展示所需的延迟加载内容，包括符号图标、警告图标、主描述、文档注释等
/// </summary>
[Export(typeof(IDeferredQuickInfoContentProvider))]
internal class DeferredQuickInfoContentProvider : IDeferredQuickInfoContentProvider
{
    /// <summary>
    /// 创建快速信息展示的延迟加载内容
    /// </summary>
    /// <param name="symbol">符号信息</param>
    /// <param name="showWarningGlyph">是否显示警告图标</param>
    /// <param name="showSymbolGlyph">是否显示符号图标</param>
    /// <param name="mainDescription">主描述文本列表</param>
    /// <param name="documentation">文档注释延迟加载内容</param>
    /// <param name="typeParameterMap">类型参数映射文本列表</param>
    /// <param name="anonymousTypes">匿名类型文本列表</param>
    /// <param name="usageText">使用说明文本列表</param>
    /// <param name="exceptionText">异常说明文本列表</param>
    /// <returns>快速信息展示的延迟加载内容实例</returns>
    public IDeferredQuickInfoContent CreateQuickInfoDisplayDeferredContent(
        ISymbol symbol,
        bool showWarningGlyph,
        bool showSymbolGlyph,
        IList<TaggedText> mainDescription,
        IDeferredQuickInfoContent documentation,
        IList<TaggedText> typeParameterMap,
        IList<TaggedText> anonymousTypes,
        IList<TaggedText> usageText,
        IList<TaggedText> exceptionText)
    {
        return new QuickInfoDisplayDeferredContent(
            symbolGlyph: showSymbolGlyph ? CreateGlyphDeferredContent(symbol) : null,
            warningGlyph: showWarningGlyph ? CreateWarningGlyph() : null,
            mainDescription: CreateClassifiableDeferredContent(mainDescription),
            documentation: documentation,
            typeParameterMap: CreateClassifiableDeferredContent(typeParameterMap),
            anonymousTypes: CreateClassifiableDeferredContent(anonymousTypes),
            usageText: CreateClassifiableDeferredContent(usageText),
            exceptionText: CreateClassifiableDeferredContent(exceptionText));
    }

    /// <summary>
    /// 创建符号图标延迟加载内容
    /// </summary>
    /// <param name="symbol">符号信息</param>
    /// <returns>符号图标延迟加载内容实例</returns>
    private static SymbolGlyphDeferredContent CreateGlyphDeferredContent(ISymbol symbol)
    {
        return new SymbolGlyphDeferredContent(symbol.GetGlyph());
    }

    /// <summary>
    /// 创建警告图标延迟加载内容
    /// </summary>
    /// <returns>警告图标延迟加载内容实例</returns>
    private static SymbolGlyphDeferredContent CreateWarningGlyph()
    {
        return new SymbolGlyphDeferredContent(Glyph.CompletionWarning);
    }

    /// <summary>
    /// 创建文档注释延迟加载内容
    /// </summary>
    /// <param name="documentationComment">文档注释文本</param>
    /// <returns>文档注释延迟加载内容实例</returns>
    public IDeferredQuickInfoContent CreateDocumentationCommentDeferredContent(string? documentationComment)
    {
        return new DocumentationCommentDeferredContent(documentationComment);
    }

    /// <summary>
    /// 创建可分类文本的延迟加载内容
    /// </summary>
    /// <param name="content">可分类文本列表</param>
    /// <returns>可分类文本延迟加载内容实例</returns>
    public IDeferredQuickInfoContent CreateClassifiableDeferredContent(IList<TaggedText> content)
    {
        return new ClassifiableDeferredContent(content);
    }

    /// <summary>
    /// 快速信息展示的延迟加载内容实现类
    /// 封装各类快速信息组件的延迟加载内容，并负责创建最终的展示面板
    /// </summary>
    /// <param name="symbolGlyph">符号图标延迟加载内容</param>
    /// <param name="warningGlyph">警告图标延迟加载内容</param>
    /// <param name="mainDescription">主描述延迟加载内容</param>
    /// <param name="documentation">文档注释延迟加载内容</param>
    /// <param name="typeParameterMap">类型参数映射延迟加载内容</param>
    /// <param name="anonymousTypes">匿名类型延迟加载内容</param>
    /// <param name="usageText">使用说明延迟加载内容</param>
    /// <param name="exceptionText">异常说明延迟加载内容</param>
    private class QuickInfoDisplayDeferredContent(
        IDeferredQuickInfoContent? symbolGlyph,
        IDeferredQuickInfoContent? warningGlyph,
        IDeferredQuickInfoContent mainDescription,
        IDeferredQuickInfoContent documentation,
        IDeferredQuickInfoContent typeParameterMap,
        IDeferredQuickInfoContent anonymousTypes,
        IDeferredQuickInfoContent usageText,
        IDeferredQuickInfoContent exceptionText) : IDeferredQuickInfoContent
    {
        private readonly IDeferredQuickInfoContent? _symbolGlyph = symbolGlyph;
        private readonly IDeferredQuickInfoContent? _warningGlyph = warningGlyph;
        private readonly IDeferredQuickInfoContent _mainDescription = mainDescription;
        private readonly IDeferredQuickInfoContent _documentation = documentation;
        private readonly IDeferredQuickInfoContent _typeParameterMap = typeParameterMap;
        private readonly IDeferredQuickInfoContent _anonymousTypes = anonymousTypes;
        private readonly IDeferredQuickInfoContent _usageText = usageText;
        private readonly IDeferredQuickInfoContent _exceptionText = exceptionText;

        /// <summary>
        /// 创建快速信息展示面板
        /// </summary>
        /// <returns>快速信息展示面板实例</returns>
        public object Create()
        {
            object? warningGlyph = null;
            if (_warningGlyph != null)
            {
                warningGlyph = _warningGlyph.Create();
            }

            object? symbolGlyph = null;
            if (_symbolGlyph != null)
            {
                symbolGlyph = _symbolGlyph.Create();
            }

            return new QuickInfoDisplayPanel(
                symbolGlyph as Control,
                warningGlyph as Control,
                (Control)_mainDescription.Create(),
                (Control)_documentation.Create(),
                (Control)_typeParameterMap.Create(),
                (Control)_anonymousTypes.Create(),
                (Control)_usageText.Create(),
                (Control)_exceptionText.Create());
        }
    }

    /// <summary>
    /// 快速信息展示面板，继承自<see cref="StackPanel"/>
    /// 负责布局和展示所有快速信息组件（图标、主描述、文档注释等）
    /// </summary>
    /// <param name="symbolGlyph">符号图标控件</param>
    /// <param name="warningGlyph">警告图标控件</param>
    /// <param name="mainDescription">主描述控件</param>
    /// <param name="documentation">文档注释控件</param>
    /// <param name="typeParameterMap">类型参数映射控件</param>
    /// <param name="anonymousTypes">匿名类型控件</param>
    /// <param name="usageText">使用说明控件</param>
    /// <param name="exceptionText">异常说明控件</param>
    private class QuickInfoDisplayPanel : StackPanel
    {
        public QuickInfoDisplayPanel(
            Control? symbolGlyph,
            Control? warningGlyph,
            Control mainDescription,
            Control documentation,
            Control typeParameterMap,
            Control anonymousTypes,
            Control usageText,
            Control exceptionText)
        {
            Orientation = Orientation.Vertical;

            Border? symbolGlyphBorder = null;
            if (symbolGlyph != null)
            {
                symbolGlyph.Margin = new Thickness(1, 1, 3, 1);
                symbolGlyphBorder = new Border()
                {
                    BorderThickness = new Thickness(),
                    BorderBrush = Brushes.Transparent,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                    Child = symbolGlyph
                };
            }

            mainDescription.Margin = new Thickness(1);
            var mainDescriptionBorder = new Border()
            {
                BorderThickness = new Thickness(),
                BorderBrush = Brushes.Transparent,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Child = mainDescription
            };

            var symbolGlyphAndMainDescriptionDock = new DockPanel()
            {
                LastChildFill = true,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Background = Brushes.Transparent
            };

            if (symbolGlyphBorder != null)
            {
                symbolGlyphAndMainDescriptionDock.Children.Add(symbolGlyphBorder);
            }

            symbolGlyphAndMainDescriptionDock.Children.Add(mainDescriptionBorder);

            if (warningGlyph != null)
            {
                warningGlyph.Margin = new Thickness(1, 1, 3, 1);
                var warningGlyphBorder = new Border()
                {
                    BorderThickness = new Thickness(),
                    BorderBrush = Brushes.Transparent,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Child = warningGlyph
                };

                symbolGlyphAndMainDescriptionDock.Children.Add(warningGlyphBorder);
            }

            Children.Add(symbolGlyphAndMainDescriptionDock);
            Children.Add(documentation);
            Children.Add(usageText);
            Children.Add(typeParameterMap);
            Children.Add(anonymousTypes);
            Children.Add(exceptionText);
        }
    }

    /// <summary>
    /// 符号图标延迟加载内容实现类
    /// 负责创建符号/警告图标的Image控件
    /// </summary>
    /// <param name="glyph">图标枚举值</param>
    private class SymbolGlyphDeferredContent(Glyph glyph) : IDeferredQuickInfoContent
    {
        /// <summary>
        /// 图标枚举值
        /// </summary>
        private Glyph Glyph { get; } = glyph;

        /// <summary>
        /// 创建图标Image控件
        /// </summary>
        /// <returns>图标Image控件实例</returns>
        public object Create()
        {
            var image = new Image
            {
                Width = 16,
                Height = 16,
                Source = Glyph.ToImageSource()!
            };
            return image;
        }
    }

    /// <summary>
    /// 可分类文本延迟加载内容实现类
    /// 负责将TaggedText列表转换为TextBlock控件
    /// </summary>
    /// <param name="content">可分类文本列表</param>
    private class ClassifiableDeferredContent(IList<TaggedText> content) : IDeferredQuickInfoContent
    {
        private readonly IList<TaggedText> _classifiableContent = content;

        /// <summary>
        /// 创建可分类文本对应的TextBlock控件
        /// </summary>
        /// <returns>TextBlock控件实例</returns>
        public object Create() => _classifiableContent.ToTextBlock();
    }

    /// <summary>
    /// 文档注释延迟加载内容实现类
    /// 负责创建展示文档注释的TextBlock控件
    /// </summary>
    /// <param name="documentationComment">文档注释文本</param>
    private class DocumentationCommentDeferredContent(string? documentationComment) : IDeferredQuickInfoContent
    {
        private readonly string? _documentationComment = documentationComment;

        /// <summary>
        /// 创建文档注释展示控件
        /// </summary>
        /// <returns>文档注释TextBlock控件实例</returns>
        public object Create()
        {
            var documentationTextBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };

            UpdateDocumentationTextBlock(documentationTextBlock);
            return documentationTextBlock;
        }

        /// <summary>
        /// 更新文档注释TextBlock的内容和可见性
        /// </summary>
        /// <param name="documentationTextBlock">需要更新的TextBlock控件</param>
        private void UpdateDocumentationTextBlock(TextBlock documentationTextBlock)
        {
            if (!string.IsNullOrEmpty(_documentationComment))
            {
                documentationTextBlock.Text = _documentationComment;
            }
            else
            {
                documentationTextBlock.Text = string.Empty;
                documentationTextBlock.IsVisible = false;
            }
        }
    }
}
