namespace RoslynPad.Editor;

/// <summary>
/// 定义上下文操作提供程序的接口，用于获取指定位置的上下文操作及对应的命令
/// </summary>
public interface IContextActionProvider
{
    /// <summary>
    /// 异步获取指定偏移量和长度范围内的上下文操作集合
    /// </summary>
    /// <param name="offset">文本偏移量</param>
    /// <param name="length">文本长度</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>异步操作结果，包含上下文操作对象的集合</returns>
    Task<IEnumerable<object>> GetActions(int offset, int length, CancellationToken cancellationToken);

    /// <summary>
    /// 获取指定上下文操作对应的命令
    /// </summary>
    /// <param name="action">上下文操作对象</param>
    /// <returns>对应的命令对象，若不存在则返回null</returns>
    ICommand? GetActionCommand(object action);
}
