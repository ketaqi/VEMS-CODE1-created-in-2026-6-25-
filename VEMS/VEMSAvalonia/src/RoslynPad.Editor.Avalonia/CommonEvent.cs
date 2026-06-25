namespace RoslynPad.Editor;

/// <summary>
/// 路由事件注册工具类，提供通用的路由事件注册方法
/// </summary>
public static class CommonEvent
{
    /// <summary>
    /// 注册路由事件
    /// </summary>
    /// <typeparam name="TOwner">事件所属者类型</typeparam>
    /// <typeparam name="TEventArgs">事件参数类型（继承自RoutedEventArgs）</typeparam>
    /// <param name="name">事件名称</param>
    /// <param name="routing">路由策略</param>
    /// <returns>注册后的路由事件实例</returns>
    public static RoutedEvent Register<TOwner, TEventArgs>(string name, RoutingStrategies routing)
        where TEventArgs : RoutedEventArgs
    {
        return RoutedEvent.Register<TOwner, TEventArgs>(name, routing);
    }
}
