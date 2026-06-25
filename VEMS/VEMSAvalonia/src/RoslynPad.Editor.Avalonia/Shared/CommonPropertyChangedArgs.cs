namespace RoslynPad.Editor;

/// <summary>
/// 通用属性变更参数类，用于传递属性变更前后的值
/// </summary>
/// <typeparam name="T">属性值的类型</typeparam>
public class CommonPropertyChangedArgs<T>(T oldValue, T newValue)
{
    /// <summary>
    /// 获取属性变更前的值
    /// </summary>
    public T OldValue { get; } = oldValue;

    /// <summary>
    /// 获取属性变更后的值
    /// </summary>
    public T NewValue { get; } = newValue;
}
