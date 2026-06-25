using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia;
using RoslynPad.FeatureDemos.Models;
using RoslynPad.UI;
using Avalonia.PropertyGrid.Controls.Factories;

namespace RoslynPad.FeatureDemos.Views
{
    internal class OnlyButtonCellEditFactory5 : AbstractCellEditFactory
    {
        // 可空，允许延迟注入
        public MainViewModel? MainWindow { get; set; }

        // 只在我们的自定义 PropertyGrid 上生效，避免影响全局
        public override bool Accept(object accessToken) => accessToken is MyPropertyGrid;

        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            // 仅为目标属性返回按钮（这里以 MyObject.Quantity 为例）
            if (context.Property.Name != nameof(SimpleObject4.Quantity5))
                return null;

            var button = new Button
            {
                Content = "RUN",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0)
            };

            button.Click += (_, _) =>
            {
                // 安全调用：如果未注入 MainWindow 或命令为 null，则不报错
                MainWindow?.CurrentOpenDocument?.RunCommand?.Execute(null);

                // 通过工厂设置属性值并刷新
                context.Factory!.SetPropertyValue(context, 42);
                context.Factory!.HandlePropertyChanged(context);
            };

            return button; // 直接让按钮作为“值单元格”控件
        }

        // 该属性不需要从模型到 UI 的同步（按钮固定），直接返回 true
        public override bool HandlePropertyChanged(PropertyCellContext context) => true;
    }
}
