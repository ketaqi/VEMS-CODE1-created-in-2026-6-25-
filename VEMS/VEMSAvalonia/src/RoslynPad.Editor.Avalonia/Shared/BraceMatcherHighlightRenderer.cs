// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using RoslynPad.Roslyn.Classification;
using RoslynPad.Roslyn.BraceMatching;
using System.Diagnostics.CodeAnalysis;

namespace RoslynPad.Editor;

/// <summary>
/// 括号匹配高亮渲染器，用于在文本视图中高亮显示匹配的括号
/// </summary>
public class BraceMatcherHighlightRenderer : IBackgroundRenderer
{
    /// <summary>
    /// 关联的文本视图实例
    /// </summary>
    private readonly TextView _textView;

    /// <summary>
    /// 分类高亮颜色配置
    /// </summary>
    private IClassificationHighlightColors _classificationHighlightColors;

    /// <summary>
    /// 括号高亮的背景画刷
    /// </summary>
    private CommonBrush _backgroundBrush;

    /// <summary>
    /// 获取或设置光标左侧的括号匹配结果
    /// </summary>
    public BraceMatchingResult? LeftOfPosition { get; private set; }

    /// <summary>
    /// 获取或设置光标右侧的括号匹配结果
    /// </summary>
    public BraceMatchingResult? RightOfPosition { get; private set; }

    /// <summary>
    /// 括号高亮的标识名称
    /// </summary>
    public const string BracketHighlight = "Bracket highlight";

    /// <summary>
    /// 初始化<BraceMatcherHighlightRenderer>实例
    /// </summary>
    /// <param name="textView">关联的文本视图，不能为空</param>
    /// <param name="classificationHighlightColors">分类高亮颜色配置，不能为空</param>
    /// <exception cref="ArgumentNullException">当textView为null时抛出</exception>
    public BraceMatcherHighlightRenderer(TextView textView, IClassificationHighlightColors classificationHighlightColors)
    {
        _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        _classificationHighlightColors = classificationHighlightColors;
        _textView.BackgroundRenderers.Add(this);
        UpdateBrush();
    }

    /// <summary>
    /// 更新括号高亮的背景画刷
    /// </summary>
    [MemberNotNull(nameof(_backgroundBrush))]
    private void UpdateBrush()
    {
        var brush = ClassificationHighlightColors
                    .GetBrush(AdditionalClassificationTypeNames.BraceMatching)
                    ?.Background?.GetBrush(null);

        if (brush != null)
        {
            _backgroundBrush = brush;
        }
        else
        {
            _backgroundBrush = Brushes.Transparent;
        }
    }

    /// <summary>
    /// 设置需要高亮的括号匹配结果
    /// </summary>
    /// <param name="leftOfPosition">光标左侧的匹配结果</param>
    /// <param name="rightOfPosition">光标右侧的匹配结果</param>
    public void SetHighlight(BraceMatchingResult? leftOfPosition, BraceMatchingResult? rightOfPosition)
    {
        if (LeftOfPosition != leftOfPosition || RightOfPosition != rightOfPosition)
        {
            LeftOfPosition = leftOfPosition;
            RightOfPosition = rightOfPosition;
            _textView.InvalidateLayer(Layer);
        }
    }

    /// <summary>
    /// 获取渲染层（选择层）
    /// </summary>
    public KnownLayer Layer => KnownLayer.Selection;

    /// <summary>
    /// 获取或设置分类高亮颜色配置，设置后会更新背景画刷
    /// </summary>
    public IClassificationHighlightColors ClassificationHighlightColors
    {
        get => _classificationHighlightColors;
        set
        {
            _classificationHighlightColors = value;
            UpdateBrush();
        }
    }

    /// <summary>
    /// 在文本视图中绘制括号高亮背景
    /// </summary>
    /// <param name="textView">文本视图实例</param>
    /// <param name="drawingContext">绘图上下文</param>
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (LeftOfPosition == null && RightOfPosition == null)
            return;

        var builder = new BackgroundGeometryBuilder
        {
            CornerRadius = 1,
#if !AVALONIA
            AlignToWholePixels = true
#endif
        };

        // 绘制光标右侧匹配结果的括号高亮
        if (RightOfPosition != null)
        {
            builder.AddSegment(textView, new TextSegment { StartOffset = RightOfPosition.Value.LeftSpan.Start, Length = RightOfPosition.Value.LeftSpan.Length });
            builder.CloseFigure();
            builder.AddSegment(textView, new TextSegment { StartOffset = RightOfPosition.Value.RightSpan.Start, Length = RightOfPosition.Value.RightSpan.Length });
            builder.CloseFigure();
        }

        // 绘制光标左侧匹配结果的括号高亮
        if (LeftOfPosition != null)
        {
            builder.AddSegment(textView, new TextSegment { StartOffset = LeftOfPosition.Value.LeftSpan.Start, Length = LeftOfPosition.Value.LeftSpan.Length });
            builder.CloseFigure();
            builder.AddSegment(textView, new TextSegment { StartOffset = LeftOfPosition.Value.RightSpan.Start, Length = LeftOfPosition.Value.RightSpan.Length });
            builder.CloseFigure();
        }

        // 绘制高亮几何图形
        var geometry = builder.CreateGeometry();
        if (geometry != null)
        {
            drawingContext.DrawGeometry(_backgroundBrush, null, geometry);
        }
    }
}
