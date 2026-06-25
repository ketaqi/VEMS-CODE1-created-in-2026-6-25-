namespace RoslynPad.Editor;

/// <summary>
/// 代码补全结果类，包含补全数据、重载提供器及选择模式信息
/// </summary>
public sealed class CompletionResult(IList<ICompletionDataEx>? completionData, IOverloadProviderEx? overloadProvider, bool useHardSelection)
{
    /// <summary>
    /// 获取是否使用硬选择模式
    /// </summary>
    public bool UseHardSelection { get; } = useHardSelection;

    /// <summary>
    /// 获取代码补全数据列表
    /// </summary>
    public IList<ICompletionDataEx>? CompletionData { get; } = completionData;

    /// <summary>
    /// 获取重载信息提供器
    /// </summary>
    public IOverloadProviderEx? OverloadProvider { get; } = overloadProvider;
}
