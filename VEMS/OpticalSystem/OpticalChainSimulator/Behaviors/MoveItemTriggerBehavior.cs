namespace OpticalChainSimulator.Behaviors;

/// <summary>
/// ListBox项移动触发行为
/// 继承自StyledElementTrigger，监听ListBox的Items集合变更事件，处理拖拽移动项时同步更新AllElements集合，
/// 并在移动完成后触发指定命令，确保ListBox显示的项与全局AllElements集合保持一致
/// </summary>
public sealed class MoveItemTriggerBehavior : StyledElementTrigger<ListBox>
{
    /// <summary>
    /// 通知项移动完成命令依赖属性
    /// 项移动（添加）完成后执行该命令，用于触发后续业务逻辑（如场景更新、状态同步等）
    /// </summary>
    public static readonly StyledProperty<ICommand?> NotifyMoveItemCompleteCommandProperty =
        AvaloniaProperty.Register<MoveItemTriggerBehavior, ICommand?>(nameof(NotifyMoveItemCompleteCommand));

    /// <summary>
    /// 所有光学元件视图模型集合依赖属性
    /// 用于存储全局光学元件VM集合，ListBox项的添加/移除会同步到该集合
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<OpticalElementViewModel>?> AllElementsProperty =
        AvaloniaProperty.Register<MoveItemTriggerBehavior, ObservableCollection<OpticalElementViewModel>?>(nameof(AllElements));

    /// <summary>
    /// 获取或设置通知项移动完成的命令
    /// 绑定到该属性的命令会在ListBox项移动（添加）完成后执行
    /// </summary>
    public ICommand? NotifyMoveItemCompleteCommand
    {
        get => GetValue(NotifyMoveItemCompleteCommandProperty);
        set => SetValue(NotifyMoveItemCompleteCommandProperty, value);
    }

    /// <summary>
    /// 获取或设置全局光学元件视图模型集合
    /// ListBox项的添加/移除操作会同步更新该集合，保持数据一致性
    /// </summary>
    public ObservableCollection<OpticalElementViewModel>? AllElements
    {
        get => GetValue(AllElementsProperty);
        set => SetValue(AllElementsProperty, value);
    }

    /// <summary>
    /// 行为附加到ListBox时执行的逻辑
    /// 订阅ListBox.Items集合的变更事件，监听项的添加/移除操作
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        // 检查关联的ListBox的Items是否支持集合变更通知，若是则订阅事件
        if (AssociatedObject?.Items is INotifyCollectionChanged collection)
            collection.CollectionChanged += OnCollectionChanged;
    }

    /// <summary>
    /// 行为从ListBox分离时执行的逻辑
    /// 取消订阅ListBox.Items集合的变更事件，避免内存泄漏
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        // 取消订阅集合变更事件，释放资源
        if (AssociatedObject?.Items is INotifyCollectionChanged collection)
            collection.CollectionChanged -= OnCollectionChanged;
    }

    /// <summary>
    /// 处理ListBox.Items集合变更事件
    /// 仅处理添加/移除操作，且仅响应拖拽移动行为，同步更新AllElements集合
    /// </summary>
    /// <param name="sender">事件发送者（ListBox.Items集合）</param>
    /// <param name="e">集合变更事件参数，包含变更类型、变更项等信息</param>
    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 仅处理添加/移除操作，忽略其他类型（如替换、重置）
        if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Remove)
            return;

        // 获取关联ListBox上的ContextDropBehavior，筛选出移动类型的拖拽处理器
        var b = Interaction.GetBehaviors(AssociatedObject!).OfType<ContextDropBehavior>().FirstOrDefault();

        // 非移动类型的拖拽行为（如复制），直接返回
        if (b?.Handler is not ItemsListBoxDropHandler { DragDropEffects: DragDropEffects.Move })
            return;

        // 全局元件集合为空，无需同步，直接返回
        if (AllElements == null)
            return;

        // 根据变更类型获取对应的光学元件VM
        var element = e.Action switch
        {
            NotifyCollectionChangedAction.Remove => e.OldItems![0] as OpticalElementViewModel, // 移除操作取旧项
            NotifyCollectionChangedAction.Add => e.NewItems?[0] as OpticalElementViewModel,    // 添加操作取新项
            _ => null
        };

        // 未获取到有效元件VM，直接返回
        if (element == null)
            return;

        // 根据变更类型同步更新AllElements集合
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                {
                    // 查找全局集合中对应ID的元件并移除
                    var f = AllElements.FirstOrDefault(a => a.Id == element.Id);
                    if (f != null)
                        AllElements.Remove(f);
                    else
                        // 未找到对应元件，输出警告日志
                        Debug.WriteLine($"⚠️ Warning: element. Id={element.Id} not found in AllElements for removal.");
                    break;
                }
            case NotifyCollectionChangedAction.Add:
                {
                    // 获取ListBox的项集合，计算新项的插入位置
                    var items = AssociatedObject!.Items;
                    var index = e.NewStartingIndex + 1;
                    // 获取插入位置的下一个元件VM（用于确定插入索引）
                    var nextVm = index == items.Count
                        ? null
                        : (OpticalElementViewModel?)items.GetAt(index);

                    if (nextVm == null)
                    {
                        // 下一个元件为空（插入到末尾），直接添加到全局集合
                        AllElements.Add(element);
                    }
                    else
                    {
                        // ✅ 修复：使用 FirstOrDefault 并检查 null，避免空引用
                        var f = AllElements.FirstOrDefault(a => a.Id == nextVm.Id);
                        if (f != null)
                        {
                            // 插入到下一个元件的前面，保持顺序一致
                            AllElements.Insert(AllElements.IndexOf(f), element);
                        }
                        else
                        {
                            // 未找到下一个元件，降级处理：添加到全局集合末尾
                            AllElements.Add(element);
                            Debug.WriteLine($"⚠️ Warning: nextVm.Id={nextVm.Id} not found in AllElements, appending to end.");
                        }
                    }
                    break;
                }
        }

        // 添加操作完成后，执行移动完成命令
        if (e.Action == NotifyCollectionChangedAction.Add)
            NotifyMoveItemCompleteCommand?.Execute(null);
    }
}