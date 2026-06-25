namespace RoslynPad.Build;

/// <summary>
/// 表示 NuGet 还原操作的结果。
/// </summary>
internal class RestoreResult
{
    /// <summary>
    /// 获取成功的还原结果单例。
    /// </summary>
    public static RestoreResult SuccessResult { get; } = new RestoreResult(success: true, errors: null);

    /// <summary>
    /// 从错误消息创建失败的还原结果。
    /// </summary>
    /// <param name="errors">错误消息数组。</param>
    /// <returns>失败的还原结果。</returns>
    public static RestoreResult FromErrors(string[] errors) => new(success: false, errors);

    /// <summary>
    /// 初始化 <see cref="RestoreResult"/> 类的新实例。
    /// </summary>
    /// <param name="success">是否成功。</param>
    /// <param name="errors">错误消息数组。</param>
    private RestoreResult(bool success, string[]? errors)
    {
        Success = success;
        Errors = errors ?? [];
    }

    /// <summary>
    /// 获取还原是否成功。
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// 获取错误消息数组。
    /// </summary>
    public string[] Errors { get; }
}
