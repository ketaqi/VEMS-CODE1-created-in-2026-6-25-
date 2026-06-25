using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.PropertyGrid.Controls;
using RoslynPad.FeatureDemos.Models;
using RoslynPad.FeatureDemos.ViewModels;
using Avalonia.PropertyGrid.Services;
using PropertyModels.ComponentModel;
using PropertyModels.Extensions;
using Avalonia.Threading;
using FeatureDemoViewModel = RoslynPad.FeatureDemos.ViewModels.FeatureDemoViewModel;
using SimpleObject = RoslynPad.FeatureDemos.Models.SimpleObject;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Views;

public partial class FeatureDemoView1 : UserControl
{

    public ICommand ShowManagedNotificationCommand { get; }

    public WindowNotificationManager? NotificationManager { get; set; }

    public FeatureDemoView1()
    {

        InitializeComponent();

        ShowManagedNotificationCommand = ReactiveCommand.Create(() =>
        {
            NotificationManager?.Show(new Notification(
                LocalizationService.Default["Welcome"],
                LocalizationService.Default["Avalonia.PropertyGrid now supports custom areas."]
                ));
        });

        CurrentPropertyName = LocalizationService.Default["No Property Focused"];

        _applyScheduled = false;

        //this.AttachedToVisualTree += (_, __) =>
        //{
        //    // 第一次安排一次应用

        ScheduleApplyExpanderHeaderFix();

        //    // 如果 PropertyGrid 可能会动态刷新生成 Expanders（后续变化），
        //    // 可以订阅 LayoutUpdated 来再次应用（用防抖 ScheduleApplyExpanderHeaderFix）
        //MyPropertyGrid1.LayoutUpdated += (_, _) => ScheduleApplyExpanderHeaderFix();
        //};

    }

    private void ApplyExpanderHeaderFixToPropertyGrid1(Control propertyGrid)
    {
        if (propertyGrid == null) return;

        // 仅在附着时做处理（可选）
        if (propertyGrid.GetVisualRoot() == null)
        {
            ScheduleApplyExpanderHeaderFix();
            return;
        }

        // 查找所有 header ToggleButton（包括后来动态创建的）
        var toggles = propertyGrid
            .GetVisualDescendants()
            .OfType<ToggleButton>()
            .ToList();

        Debug.WriteLine($"ApplyExpanderHeaderFix: found toggles count = {toggles.Count}");

        foreach (var tb in toggles)
        {
            // 防护：确保 tb 非空
            if (tb == null) continue;

            // 找到该 ToggleButton 的 Expander 祖先链
            var expanderAncestors = tb.GetVisualAncestors().OfType<Expander>().ToList();

            // 顶级分类：祖先 Expander 数量 == 1（ToggleButton 的第一个 Expander 祖先是自己那一层）
            // 子分类（嵌套）：祖先 Expander 数量 >= 2
            var isSubCategoryHeader = expanderAncestors.Count >= 2;

            // 取得最接近的 Expander（closest ancestor）用于获取主题 Brush（或 null）
            var nearestExpander = expanderAncestors.FirstOrDefault();

            // 已有的通过资源读取 Brush 的方法（接受 Expander）
            var brush = nearestExpander != null ? GetThemeBrushFromResources(nearestExpander) : null;
            // fallback
            if (brush == null)
                brush = Brushes.LightBlue; // 或者从 Application.Current.Resources 中读取

            if (isSubCategoryHeader)
            {
                tb.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                tb.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                tb.Background = brush;
                // 可以设置 Padding 等
                // tb.Padding = new Thickness(6);
            }
            else
            {
                // 如果想对顶级分类也统一处理，可以在这里设置
                // tb.Background = someOtherBrush;
            }
        }
    }

    private void ScheduleApplyExpanderHeaderFix()
    {
        if (_applyScheduled) return;
        _applyScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                ApplyExpanderHeaderFixToPropertyGrid(MyPropertyGrid1);
            }
            finally
            {
                _applyScheduled = false;
            }
        }, DispatcherPriority.Render);
    }

    private bool _applyScheduled;


    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // 确保 DataContext 初始化（MainWindow 与其 DataContext 已就绪）
        EnsureDataContext();

        NotificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this)!);
    }

    private void EnsureDataContext()
    {
        // 已经有 FeatureDemoViewModel 就不覆盖
        if (DataContext is FeatureDemoViewModel) return;

        // 优先从当前 TopLevel 的 DataContext 获取 MainViewModel
        var mainVm = TopLevel.GetTopLevel(this)?.DataContext as MainViewModel
                     ?? (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                         ?.MainWindow?.DataContext as MainViewModel;
        
        if (mainVm is not null)
        {
            DataContext = new FeatureDemoViewModel(mainVm);
        }
        else
        {
            // 可能仍未就绪：异步重试一次，避免在构造阶段抛异常
            Dispatcher.UIThread.Post(EnsureDataContext, DispatcherPriority.Background);
        }
    }

    private void OnCustomPropertyDescriptorFilter(object sender, RoutedEventArgs args)
    {
        if (args is CustomPropertyDescriptorFilterEventArgs { TargetObject: SimpleObject, PropertyDescriptor.Name: nameof(SimpleObject.ThreeStates2) } e)
        {
            e.IsVisible = true;
            e.Handled = true;
        }
    }

    private void OnCommandExecuted(object? sender, RoutedEventArgs e) => (DataContext as FeatureDemoViewModel)!.CommandHistory.PushCommand((e as RoutedCommandExecutedEventArgs)!.Command);

    public static readonly StyledProperty<string> CurrentPropertyNameProperty =
        AvaloniaProperty.Register<FeatureDemoView1, string>(nameof(CurrentPropertyName));

    public string CurrentPropertyName
    {
        get => GetValue(CurrentPropertyNameProperty);
        set => SetValue(CurrentPropertyNameProperty, value);
    }

    private void OnPropertyGotFocus(object? sender, RoutedEventArgs args)
    {
        var e = args as PropertyGotFocusEventArgs;

#pragma warning disable CA1305
        CurrentPropertyName =
            string.Format(LocalizationService.Default["CurrentPropertyDescription"],
                LocalizationService.Default[e!.Context.DisplayName],
                e.Context.Property.Description.IsNotNullOrEmpty() ? (": " + LocalizationService.Default[e.Context.Property.Description]) : ""
                );
#pragma warning restore CA1305
    }

    private void OnPropertyLostFocus(object? sender, RoutedEventArgs args)
    {
        CurrentPropertyName = LocalizationService.Default["No Property Focused"];
    }

    private void Expander_TemplateApplied_ForSamples(object? sender, TemplateAppliedEventArgs? args)
    {
        if (sender is not Expander expander) return;

        // 找到模板里的 ToggleButton（通常作为 Header 的宿主）

        var headerToggle = expander.GetTemplateChildren().OfType<ToggleButton>().FirstOrDefault();

        if (headerToggle != null)
        {

            headerToggle.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

            headerToggle.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

            // 可设置背景以在 Header 上显示整行背景（或在 ContentPresenter 上设置）

            //headerToggle.Background = Brushes.Red; // 或想要的颜色

            var themeBrush = GetThemeBrushFromResources(expander);

            headerToggle.Background = themeBrush; // 或想要的颜色
           
        }
    }

    private IBrush? GetThemeBrushFromResources(Expander expander)
    {
        const string key = ThemeDictionary.documenttree; // "document.tree"

        // 1) 尝试从 Application 全局资源查找（包括合并字典）
        if (Application.Current?.Resources is ResourceDictionary appRes
            && appRes.TryGetValue(key, out var appVal)
            && appVal is IBrush appBrush)
        {
            return appBrush;
        }

        // 2) 尝试从控件的资源 / 样式上下文查找（更接近控件实际使用的资源）
        //if (expander is IResourceProvider rp
        //    && rp.TryGetResource(key, CultureInfo.CurrentUICulture, out var localVal)
        //    && localVal is IBrush localBrush)
        //{
        //    return localBrush;
        //}

        // 未找到
        return null;
    }

    private void ApplyExpanderHeaderFixToPropertyGrid(Control propertyGrid)
    {
        Debug.WriteLine($"Apply called. propertyGrid is null: {propertyGrid == null}");
        if (propertyGrid == null) return;
        Debug.WriteLine($"IsAttachedToVisualTree: {propertyGrid.IsAttachedToVisualTree}");
        // 遍历可视子树，找到所有 Expander
        var expanders = propertyGrid.GetVisualDescendants().OfType<Expander>().ToList();

        var propsGrid = propertyGrid.FindControl<Grid>("PropertiesGrid"); 

        var expanders1 = propsGrid?.Children.OfType<Expander>().ToList();
        string targetHeaderText = "自动折叠";

        // 找到 Header 为 "自动折叠" 的 Expander（尽量稳健地检查多种可能）
        var targetExpander = expanders.FirstOrDefault(e =>
        {
            // 1) 直接看 Header 属性
            if (e.Header != null)
            {
                var h = e.Header.ToString();
                if (!string.IsNullOrEmpty(h) && h == targetHeaderText) return true;
            }

            // 2) 尝试从模板找到 ToggleButton，然后在 ToggleButton 的视觉后代中查找文本为目标的 TextBlock
            try
            {
                var toggle = e.GetTemplateChildren().OfType<ToggleButton>().FirstOrDefault();
                if (toggle != null)
                {
                    var tbInToggle = toggle.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Text == targetHeaderText);
                    if (tbInToggle != null) return true;
                }
            }
            catch
            {
                // 某些平台/版本 GetTemplateChildren 可能抛异常，忽略并继续检查视觉后代
            }

            // 3) 最后在 Expander 的视觉后代中查找文本匹配的 TextBlock（保底）
            var tbAny = e.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Text == targetHeaderText);
            if (tbAny != null) return true;

            return false;
        });

       
        if (targetExpander == null)
        {
            Debug.WriteLine($"没有找到 Header 为 '{targetHeaderText}' 的 Expander。");
        }
        else
        {
            Debug.WriteLine($"找到目标 Expander Header='{targetHeaderText}', Hash={targetExpander.GetHashCode()}, IsAttached={targetExpander.IsAttachedToVisualTree}");

            // 所有后代 Expander（不包括自己）
            var allDescendants = targetExpander.GetVisualDescendants().OfType<Expander>().Where(x => x != targetExpander).ToList();
            int totalDescendantCount = allDescendants.Count;

            // 直接子 Expander：取所有后代中其最近的 Expander 祖先如果是 targetExpander 则视为直接子
            var directChildren = allDescendants
                .Where(d =>
                {
                    var nearest = d.GetVisualAncestors().OfType<Expander>().FirstOrDefault();
                    return nearest == targetExpander;
                })
                .ToList();
            int directChildCount = directChildren.Count;

            Debug.WriteLine($"目标 Expander 的总后代 Expander 数量 = {totalDescendantCount}, 直接子 Expander 数量 = {directChildCount}");

            // 可选：打印直接子项的 header
            //foreach (var child in directChildren)
            //{
            //    // 试着读取 child.Header 或其模板中文本
            //    string childHeader = child.Header?.ToString() ?? "(no header)";
            //    var tb = child.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault();
            //    if (tb != null && !string.IsNullOrEmpty(tb.Text)) childHeader = tb.Text;
            //    Debug.WriteLine($"  直接子 Expander: Header='{childHeader}', Hash={child.GetHashCode()}");
            //}
        }
        foreach (var expander in expanders)
        {
            // 防止重复附加
            expander.TemplateApplied -= Expander_TemplateApplied_ForSamples;
            expander.TemplateApplied += Expander_TemplateApplied_ForSamples;

            // 如果模板子元素已经存在（模板已应用），就直接执行一次处理逻辑
            // 通过 GetTemplateChildren() 判断是否已生成模板子元素
            try
            {
                var templateChildren = expander.GetTemplateChildren();
                if (templateChildren != null && templateChildren.Any())
                {
                    // 立即调用一次修补（传 null/ default 给第二个参数即可）
                    Expander_TemplateApplied_ForSamples(expander, default);
                }
            }
            catch
            {
                // 在某些环境或版本中 GetTemplateChildren 可能抛异常，作为兜底可直接调用处理器一次
                Expander_TemplateApplied_ForSamples(expander, default);
            }
        }
    }

}
