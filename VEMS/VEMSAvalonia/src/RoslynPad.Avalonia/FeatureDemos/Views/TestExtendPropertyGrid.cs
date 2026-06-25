using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using Avalonia.PropertyGrid.Controls.Factories.Builtins;
using Avalonia.PropertyGrid.Localization;
using Avalonia.PropertyGrid.Services;
using Avalonia.VisualTree;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using PropertyModels.Extensions;
using RoslynPad.FeatureDemos.Models;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Views
{
    /// <summary>
    /// 扩展的属性网格控件，用于演示自定义单元格编辑工厂功能。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此控件继承自 <see cref="PropertyGrid"/>，并注册了多个自定义单元格编辑工厂：
    /// <list type="bullet">
    ///   <item><description><see cref="CountryInfoCellEditFactory"/> - 国家信息选择器</description></item>
    ///   <item><description><see cref="ToggleSwitchCellEditFactory"/> - 开关样式的布尔值编辑器</description></item>
    ///   <item><description><see cref="OnlyButtonCellEditFactorytest"/> - 基��特性的按钮编辑器</description></item>
    ///   <item><description><see cref="HotkeyCellEditFactory"/> - 快捷键编辑器</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 还支持自定义名称块和属性操作控件/菜单。
    /// </para>
    /// </remarks>
    public class TestExtendPropertyGrid : PropertyGrid
    {
        /// <summary>
        /// 自定义标签点击计数器。
        /// </summary>
        private int _customLabelClickCount;

        /// <summary>
        /// 标记按钮工厂是否已添加主视图模型。
        /// </summary>
        private bool _onlyButtonFactoryAdded;

        /// <summary>
        /// 初始化 <see cref="TestExtendPropertyGrid"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// 此构造函数执行以下初始化操作：
        /// <list type="number">
        ///   <item><description>创建并注册所有自定义单元格编辑工厂</description></item>
        ///   <item><description>配置视觉树附着事件以注入 MainViewModel</description></item>
        ///   <item><description>设置自定义名称块事件处理</description></item>
        ///   <item><description>设置自定义属性操作控件和菜单事件处理</description></item>
        /// </list>
        /// </remarks>
        public TestExtendPropertyGrid()
        {
            // 创建按钮工厂实例
            var onlyButtonFactorytest = new OnlyButtonCellEditFactorytest();

            // 注册所有自定义工厂
            Factories.AddFactory(new CountryInfoCellEditFactory());
            Factories.AddFactory(new ToggleSwitchCellEditFactory());
            Factories.AddFactory(onlyButtonFactorytest);
            Factories.AddFactory(new HotkeyCellEditFactory());

            // 当控件附着到视觉树时注入 MainViewModel
            AttachedToVisualTree += (s, e) =>
            {
                if (_onlyButtonFactoryAdded)
                {
                    return;
                }

                // 尝试从顶层窗口的 DataContext 获取 MainViewModel
                if (this.GetVisualRoot() is Window window && window.DataContext is MainViewModel mainViewModel)
                {
                    onlyButtonFactorytest.MainWindow = mainViewModel;
                }

                _onlyButtonFactoryAdded = true;
            };

            // 设置自定义名称块事件处理
            CustomNameBlock += OnCustomNameBlock;

            // 设置自定义属性操作事件处理
            CustomPropertyOperationControl += OnCustomPropertyOperationControl;
            CustomPropertyOperationMenuOpening += OnCustomPropertyOperationMenuOpening;
        }

        /// <summary>
        /// 处理自定义名称块事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void OnCustomNameBlock(object? sender, RoutedEventArgs e)
        {
            if (e is CustomNameBlockEventArgs { Context.Property.Name: nameof(TestExtendsObject.customLabel) } eventArgs)
            {
                var button = new Button
                {
                    Content = LocalizationService.Default[eventArgs.Context.Property.DisplayName]
                };

                button.Click += (_, _) =>
                {
                    button.Content = $"{LocalizationService.Default[eventArgs.Context.Property.DisplayName]} {++_customLabelClickCount}";
                };

                // 监听语言变更事件
                LocalizationService.Default.OnCultureChanged += (_, _) =>
                {
                    button.Content = $"{LocalizationService.Default[eventArgs.Context.Property.DisplayName]} {_customLabelClickCount}";
                };

                eventArgs.CustomNameBlock = button;
            }
        }

        /// <summary>
        /// 处理自定义属性操作菜单打开事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="args">事件参数。</param>
        private static void OnCustomPropertyOperationMenuOpening(object? sender, RoutedEventArgs args)
        {
            if (args is not CustomPropertyDefaultOperationEventArgs e)
            {
                return;
            }

            if (!e.Context.Property.IsDefined<PropertyOperationVisibilityAttribute>())
            {
                return;
            }

            if (e.Context.Property.Name != nameof(TestExtendsObject.CustomOperationMenuNumber))
            {
                return;
            }

            switch (e.StageType)
            {
                case PropertyDefaultOperationStageType.Init:
                    e.DefaultButton.SetLocalizeBinding(ContentProperty, "Operation");
                    break;

                case PropertyDefaultOperationStageType.MenuOpening:
                    CreateOperationMenu(e);
                    break;
            }
        }

        /// <summary>
        /// 创建属性操作菜单项。
        /// </summary>
        /// <param name="e">事件参数。</param>
        private static void CreateOperationMenu(CustomPropertyDefaultOperationEventArgs e)
        {
            e.Menu.Items.Clear();

            // 最小值菜单项
            var minMenuItem = new MenuItem();
            minMenuItem.SetLocalizeBinding(HeaderedSelectingItemsControl.HeaderProperty, "Min");
            minMenuItem.Click += (_, _) =>
            {
                e.Context.Factory!.SetPropertyValue(e.Context, 0);
            };

            // 最大值菜单项
            var maxMenuItem = new MenuItem();
            maxMenuItem.SetLocalizeBinding(HeaderedSelectingItemsControl.HeaderProperty, "Max");
            maxMenuItem.Click += (_, _) =>
            {
                e.Context.Factory!.SetPropertyValue(e.Context, 1024);
            };

            // 错误值菜单项（用于测试验证）
            var errorMenuItem = new MenuItem();
            errorMenuItem.SetLocalizeBinding(HeaderedSelectingItemsControl.HeaderProperty, "GenError");
            errorMenuItem.Click += (_, _) =>
            {
                e.Context.Factory!.SetPropertyValue(e.Context, 1024000);
            };

            e.Menu.Items.Add(minMenuItem);
            e.Menu.Items.Add(maxMenuItem);
            e.Menu.Items.Add(errorMenuItem);
        }

        /// <summary>
        /// 处理自定义属性操作控件事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="args">事件参数。</param>
        private static void OnCustomPropertyOperationControl(object? sender, RoutedEventArgs args)
        {
            if (args is not CustomPropertyOperationControlEventArgs e)
            {
                return;
            }

            if (!e.Context.Property.IsDefined<PropertyOperationVisibilityAttribute>())
            {
                return;
            }

            if (e.Context.Property.Name != nameof(TestExtendsObject.CustomOperationControlNumber))
            {
                return;
            }

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // 最小值按钮
            var minButton = new Button();
            minButton.SetLocalizeBinding(ContentProperty, "Min");
            minButton.Click += (_, _) =>
            {
                e.Context.Factory!.SetPropertyValue(e.Context, 0);
            };
            stackPanel.Children.Add(minButton);

            // 最大值按钮
            var maxButton = new Button();
            maxButton.SetLocalizeBinding(ContentProperty, "Max");
            maxButton.Click += (_, _) =>
            {
                e.Context.Factory!.SetPropertyValue(e.Context, 1024);
            };
            stackPanel.Children.Add(maxButton);

            e.CustomControl = stackPanel;
        }
    }

    #region SVector3 相关类型

    /// <summary>
    /// 三维向量结构体的视图模型包装器。
    /// </summary>
    /// <remarks>
    /// 此类为 <see cref="SVector3"/> 结构体提供属性变更通知支持，
    /// 以便在属性网格中进行数据绑定。
    /// </remarks>
    public class SVector3ViewModel : MiniReactiveObject
    {
#pragma warning disable CA1051 // 不要声明可见实例字段
        /// <summary>
        /// 被包装的向量结构体。
        /// </summary>
        public SVector3 Vec;
#pragma warning restore CA1051

        /// <summary>
        /// 获取或设置 X 分量。
        /// </summary>
        public float X
        {
            get => Vec.x;
            set => this.RaiseAndSetIfChanged(ref Vec.x, value);
        }

        /// <summary>
        /// 获取或设置 Y 分量。
        /// </summary>
        public float Y
        {
            get => Vec.y;
            set => this.RaiseAndSetIfChanged(ref Vec.y, value);
        }

        /// <summary>
        /// 获取或设置 Z 分量。
        /// </summary>
        public float Z
        {
            get => Vec.z;
            set => this.RaiseAndSetIfChanged(ref Vec.z, value);
        }
    }

    /// <summary>
    /// 三维向量单元格编辑工厂（未实现）。
    /// </summary>
    /// <remarks>
    /// 此类为未来实现预留，用于在属性网格中提供自定义的向量编辑控件。
    /// </remarks>
    internal class Vector3CellEditFactory
    {
        /// <summary>
        /// 确定此工厂是否接受指定的访问令牌。
        /// </summary>
        /// <param name="accessToken">访问令牌对象。</param>
        /// <returns>若访问令牌是 <see cref="TestExtendPropertyGrid"/> 实例则返回 <see langword="true"/>。</returns>
        public bool Accept(object accessToken) => accessToken is TestExtendPropertyGrid;
    }

    #endregion

    #region 国家信息相关类型

    /// <summary>
    /// 国家信息可选择列表单元格编辑工厂。
    /// </summary>
    /// <remarks>
    /// 此工厂为 <see cref="SelectableList{T}"/>（其中 T 为 <see cref="CountryInfo"/>）
    /// 类型的属性提供带国旗图标的下拉选择控件。
    /// </remarks>
    internal class CountryInfoCellEditFactory : SelectableListCellEditFactory
    {
        /// <summary>
        /// 获取工厂的导入优先级。
        /// </summary>
        /// <value>基类优先级加 100。</value>
        public override int ImportPriority => base.ImportPriority + 100;

        /// <summary>
        /// 确定此工厂是否接受指定的访问令牌。
        /// </summary>
        /// <param name="accessToken">访问令牌对象。</param>
        /// <returns>若访问令牌是 <see cref="TestExtendPropertyGrid"/> 实例则返回 <see langword="true"/>。</returns>
        public override bool Accept(object accessToken) => accessToken is TestExtendPropertyGrid;
    }

    #endregion

    #region 布尔值相关类型

    /// <summary>
    /// 开关样式的布尔值单元格编辑工厂。
    /// </summary>
    /// <remarks>
    /// 此工厂为布尔类型属性提供 <see cref="ToggleSwitch"/> 控件，
    /// 替代默认的复选框样式。
    /// </remarks>
    internal class ToggleSwitchCellEditFactory : AbstractCellEditFactory
    {
        /// <summary>
        /// 确定此工厂是否接受指定的访问令牌。
        /// </summary>
        /// <param name="accessToken">访问令牌对象。</param>
        /// <returns>若访问令牌是 <see cref="TestExtendPropertyGrid"/> 实例则返回 <see langword="true"/>。</returns>
        public override bool Accept(object accessToken) => accessToken is TestExtendPropertyGrid;

        /// <summary>
        /// 为布尔类型属性创建开关控件。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <returns>
        /// 若属性类型为 <see cref="bool"/>，返回配置好的 <see cref="ToggleSwitch"/> 控件；
        /// 否则返回 <see langword="null"/>。
        /// </returns>
        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            var propertyDescriptor = context.Property;

            if (propertyDescriptor.PropertyType != typeof(bool))
            {
                return null;
            }

            var control = new ToggleSwitch();
            control.SetLocalizeBinding(ToggleSwitch.OnContentProperty, "On");
            control.SetLocalizeBinding(ToggleSwitch.OffContentProperty, "Off");

            control.IsCheckedChanged += (s, e) => SetAndRaise(context, control, control.IsChecked);

            return control;
        }

        /// <summary>
        /// 处理属性值变更，同步到开关控件。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <returns>若成功处理返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            var propertyDescriptor = context.Property;
            var target = context.Target;
            var control = context.CellEdit!;

            if (propertyDescriptor.PropertyType != typeof(bool))
            {
                return false;
            }

            ValidateProperty(control, propertyDescriptor, target);

            if (control is ToggleSwitch toggleSwitch)
            {
                toggleSwitch.IsChecked = (bool)propertyDescriptor.GetValue(target)!;
                return true;
            }

            return false;
        }
    }

    #endregion
}
