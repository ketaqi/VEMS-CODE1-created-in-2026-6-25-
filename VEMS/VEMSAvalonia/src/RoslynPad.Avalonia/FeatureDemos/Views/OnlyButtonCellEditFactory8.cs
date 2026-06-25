using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia;
using RoslynPad.FeatureDemos.Models;
using RoslynPad.UI;
using Avalonia.PropertyGrid.Controls.Factories;
using Avalonia.PropertyGrid.Services;

namespace RoslynPad.FeatureDemos.Views
{
    internal class OnlyButtonCellEditFactory8 : AbstractCellEditFactory
    {
        // 可空，允许延迟注入
        public MainViewModel? MainWindow { get; set; }

        // 只在我们的自定义 PropertyGrid 上生效，避免影响全局
        public override bool Accept(object accessToken) => accessToken is MyPropertyGrid;

        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            // 仅为目标属性返回按钮（这里以 MyObject.Quantity 为例）
            if (context.Property.Name != nameof(SimpleObject3.leftMenuItem66))
                return null;

            var button = new Button
            {
                Content = "RUNNNNNNNNNNNN",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0)
            };

            button.Click += (_, _) =>
            {
                // 安全调用：如果未注入 MainWindow 或命令为 null，则不报错
                MainWindow?.SetDefaultPlatformCommand?.Execute(null);
                var svc = LocalizationService.Default as Avalonia.PropertyGrid.Localization.AssemblyJsonAssetLocalizationService;
                Console.WriteLine($"[Localization Debug] Default service exists: {svc != null}");
                if (svc != null)
                {
                    // 反射或访问内部字段会更难，最好直接实例化一个新的 service 指向预期 assembly 或 uri（下面也给示例）
                    // 但我们可以直接打印 GetCultures 结果:cc
                    var cultures = svc.GetCultures();
                    Console.WriteLine($"[Localization Debug] GetCultures returned: {cultures?.Length ?? 0}");
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    foreach (var c in cultures)
                        Console.WriteLine($"[Localization Debug] culture: {c.Culture?.Name} path: {c.Path}");
#pragma warning restore CS8602 // 解引用可能出现空引用。

                }
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
