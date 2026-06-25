namespace RoslynPad.Editor;

/// <summary>
/// 扩展<see cref="ICompletionData"/>接口，补充代码补全的选中状态和排序文本信息
/// </summary>
public interface ICompletionDataEx : ICompletionData
{
    /// <summary>
    /// 获取一个值，指示当前补全项是否被选中
    /// </summary>
    bool IsSelected { get; }

    /// <summary>
    /// 获取用于补全项排序的文本
    /// </summary>
    string SortText { get; }
}
