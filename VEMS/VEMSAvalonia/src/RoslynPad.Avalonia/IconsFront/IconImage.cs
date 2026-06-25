using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

#nullable enable

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 表示可绘制的图标图像。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类继承自 <see cref="DrawingImage"/>，提供基于 SVG 路径的图标渲染功能。
    /// 支持自定义画笔和画笔颜色。
    /// </para>
    /// <para>
    /// 可用于在任何支持 <see cref="IImage"/> 的控件中显示图标，
    /// 如 <see cref="Avalonia.Controls. Image"/>。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var iconImage = new IconImage("fa-solid fa-user", Brushes.Blue);
    /// imageControl.Source = iconImage;
    /// </code>
    /// </example>
    public class IconImage : DrawingImage, IImage
    {
        /// <summary>
        /// 标识 <see cref="Value"/> 依赖属性。
        /// </summary>
        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<IconImage, string>(nameof(Value), string.Empty);

        /// <summary>
        /// 标识 <see cref="Brush"/> 依赖属性。
        /// </summary>
        public static readonly StyledProperty<IBrush> BrushProperty =
            AvaloniaProperty.Register<IconImage, IBrush>(nameof(Brush), new SolidColorBrush(Colors.Black));

        /// <summary>
        /// 标识 <see cref="Pen"/> 依赖属性。
        /// </summary>
        public static readonly StyledProperty<IPen> PenProperty =
            AvaloniaProperty.Register<IconImage, IPen>(nameof(Pen), new ImmutablePen(Colors.Black.ToUInt32(), 0));

        /// <summary>
        /// 图标的边界矩形。
        /// </summary>
        private Rect _bounds;

        /// <summary>
        /// 初始化 <see cref="IconImage"/> 类的新实例（使用默认值）。
        /// </summary>
        public IconImage()
            : this(string.Empty, new SolidColorBrush(Colors.Black))
        {
        }

        /// <summary>
        /// 使用指定的图标值和画笔初始化 <see cref="IconImage"/> 类的新实例。
        /// </summary>
        /// <param name="value">图标标识字符串。</param>
        /// <param name="brush">填充图标的画笔。</param>
        public IconImage(string value, IBrush brush)
        {
            Value = value;
            Brush = brush;
        }

        /// <summary>
        /// 获取或设置图标标识值。
        /// </summary>
        /// <value>图标标识字符串（如 "fa-solid fa-user"）。</value>
        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// 获取或设置填充图标的画笔。
        /// </summary>
        /// <value>用于填充图标的 <see cref="IBrush"/>。</value>
        public IBrush Brush
        {
            get => GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        /// <summary>
        /// 获取或设置图标的描边画笔。
        /// </summary>
        /// <value>用于描边的 <see cref="IPen"/>。</value>
        public IPen Pen
        {
            get => GetValue(PenProperty);
            set => SetValue(PenProperty, value);
        }

        /// <inheritdoc/>
        /// <value>图标图像的尺寸。</value>
        public new Size Size => _bounds.Size;

        /// <inheritdoc/>
        Size IImage.Size => _bounds.Size;

        /// <summary>
        /// 当属性值变更时调用。
        /// </summary>
        /// <param name="change">属性变更事件参数。</param>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty)
            {
                HandleValueChanged();
                RaiseInvalidated(EventArgs.Empty);
            }
            else if (change.Property == BrushProperty)
            {
                HandleBrushChanged();
                RaiseInvalidated(EventArgs.Empty);
            }
            else if (change.Property == PenProperty)
            {
                HandlePenChanged();
                RaiseInvalidated(EventArgs.Empty);
            }
        }

        /// <summary>
        /// 处理图标值变更，更新几何数据。
        /// </summary>
        private void HandleValueChanged()
        {
            var iconModel = IconProvider.Current.GetIcon(Value);

            _bounds = new Rect(
                x: iconModel.ViewBox.X,
                y: iconModel.ViewBox.Y,
                width: iconModel.ViewBox.Width,
                height: iconModel.ViewBox.Height);

            var drawing = GetGeometryDrawing();
            drawing.Geometry = StreamGeometry.Parse(iconModel.Path);
        }

        /// <summary>
        /// 处理画笔变更。
        /// </summary>
        private void HandleBrushChanged()
        {
            var drawing = GetGeometryDrawing();
            drawing.Brush = Brush;
        }

        /// <summary>
        /// 处理描边画笔变更。
        /// </summary>
        private void HandlePenChanged()
        {
            var drawing = GetGeometryDrawing();
            drawing.Pen = Pen;
        }

        /// <summary>
        /// 获取或创建几何绘图对象。
        /// </summary>
        /// <returns><see cref="GeometryDrawing"/> 实例。</returns>
        private GeometryDrawing GetGeometryDrawing()
        {
            return (GeometryDrawing)(Drawing ??= new GeometryDrawing());
        }

        /// <inheritdoc/>
        void IImage.Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            var drawing = Drawing;
            if (drawing == null)
            {
                return;
            }

            var bounds = _bounds;
            var scale = Matrix.CreateScale(
                destRect.Width / sourceRect.Width,
                destRect.Height / sourceRect.Height);
            var translate = Matrix.CreateTranslation(
                -sourceRect.X + destRect.X - bounds.X,
                -sourceRect.Y + destRect.Y - bounds.Y);

            using (context.PushClip(destRect))
            using (context.PushTransform(translate * scale))
            {
                drawing.Draw(context);
            }
        }
    }
}
